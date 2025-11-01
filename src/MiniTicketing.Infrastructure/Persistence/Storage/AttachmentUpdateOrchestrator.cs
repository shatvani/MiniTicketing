using Microsoft.Extensions.Logging;
using MiniTicketing.Application.Features.Tickets.Shared;
using MiniTicketing.Application.Abstractions.Services;
using MiniTicketing.Domain.Entities;

namespace MiniTicketing.Infrastructure.Persistence.Storage;

public sealed class AttachmentUpdateOrchestrator : IAttachmentUpdateOrchestrator
{
    private readonly IAttachmentStagingService _staging;
    private readonly ILogger<AttachmentUpdateOrchestrator> _logger;

    public AttachmentUpdateOrchestrator(
        IAttachmentStagingService staging,
        ILogger<AttachmentUpdateOrchestrator> logger)
    {
        _staging = staging;
        _logger = logger;
    }

    public async Task ApplyAsync(Ticket ticket, AttachmentChangeSet changeSet, CancellationToken ct)
    {
        // “unit of work” jelleggel kezeljük
        var deletedFiles = new List<(string originalPath, string deletedPath, TicketAttachment attachment)>();
        var stagedAddedFiles = new List<(string path, TicketAttachment attachment)>();

        try
        {
            await StageDeletesAsync(ticket, changeSet, deletedFiles, ct);
            await StageAddsAsync(ticket, changeSet, stagedAddedFiles, ct);
            await FinalizeDeletesAsync(deletedFiles, ct);
            await FinalizeAddsAsync(stagedAddedFiles, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Attachment update failed for ticket {TicketId}. Rolling back...", ticket.Id);
            await RollbackAddsAsync(ticket, stagedAddedFiles, ct);
            await RollbackDeletesAsync(ticket, deletedFiles, ct);
            throw;
        }
    }

    private async Task StageDeletesAsync(
        Ticket ticket,
        AttachmentChangeSet changeSet,
        List<(string originalPath, string deletedPath, TicketAttachment attachment)> deletedFiles,
        CancellationToken ct)
    {
        foreach (var attachment in changeSet.ToRemove)
        {
            if (string.IsNullOrWhiteSpace(attachment.Path))
                continue;

            var original = attachment.Path;
            var deleted = $"deleted/{attachment.Path}";

            await _staging.CopyAttachmentFilesAsync(original, deleted, ct);
            await _staging.RemoveAttachmentFilesAsync(original, ct);

            ticket.TicketAttachments?.Remove(attachment);

            deletedFiles.Add((original, deleted, attachment));

            _logger.LogInformation("Staged delete for attachment {AttachmentId}", attachment.Id);
        }
    }

    private async Task StageAddsAsync(
        Ticket ticket,
        AttachmentChangeSet changeSet,
        List<(string path, TicketAttachment attachment)> stagedAddedFiles,
        CancellationToken ct)
    {
        foreach (var item in changeSet.ToAdd)
        {
            var attachment = item.Attachment;
            var file = item.File;

            await _staging.AddAttachmentFilesAsync((attachment.Path!, file), ct);

            ticket.TicketAttachments ??= new List<TicketAttachment>();
            ticket.TicketAttachments.Add(attachment);

            stagedAddedFiles.Add((attachment.Path!, attachment));

            _logger.LogInformation("Staged add for attachment {AttachmentId}", attachment.Id);
        }
    }

    private async Task FinalizeDeletesAsync(
        List<(string originalPath, string deletedPath, TicketAttachment attachment)> deletedFiles,
        CancellationToken ct)
    {
        foreach (var df in deletedFiles)
        {
            await _staging.RemoveAttachmentFilesAsync(df.deletedPath, ct);
        }
    }

    private async Task FinalizeAddsAsync(
        List<(string path, TicketAttachment attachment)> stagedAddedFiles,
        CancellationToken ct)
    {
        foreach (var af in stagedAddedFiles)
        {
            var finalPath = af.path;
            var stagedPath = $"added/{af.path}";

            await _staging.CopyAttachmentFilesAsync(stagedPath, finalPath, ct);
            await _staging.RemoveAttachmentFilesAsync(stagedPath, ct);
        }
    }

    private async Task RollbackAddsAsync(
        Ticket ticket,
        List<(string path, TicketAttachment attachment)> stagedAddedFiles,
        CancellationToken ct)
    {
        foreach (var added in stagedAddedFiles)
        {
            var stagedPath = $"added/{added.path}";

            try
            {
                await _staging.RemoveAttachmentFilesAsync(stagedPath, ct);
            }
            catch { /* best-effort */ }

            ticket.TicketAttachments?.Remove(added.attachment);
        }
    }

    private async Task RollbackDeletesAsync(
        Ticket ticket,
        List<(string originalPath, string deletedPath, TicketAttachment attachment)> deletedFiles,
        CancellationToken ct)
    {
        foreach (var deleted in deletedFiles)
        {
            try
            {
                await _staging.CopyAttachmentFilesAsync(deleted.deletedPath, deleted.originalPath, ct);
                await _staging.RemoveAttachmentFilesAsync(deleted.deletedPath, ct);
            }
            catch { /* best-effort */ }

            if (ticket.TicketAttachments?.Any(a => a.Id == deleted.attachment.Id) != true)
            {
                ticket.TicketAttachments ??= new List<TicketAttachment>();
                ticket.TicketAttachments.Add(deleted.attachment);
            }
        }
    }
}
