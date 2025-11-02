using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiniTicketing.Application.Abstractions.Services;
using MiniTicketing.Application.Features.Tickets.Shared;
using MiniTicketing.Domain.Entities;
using MiniTicketing.Infrastructure.Persistence;

namespace MiniTicketing.Infrastructure.Persistence.Storage;

public sealed class AttachmentUpdateOrchestrator : IAttachmentUpdateOrchestrator
{
    private readonly IAttachmentStagingService _staging;
    private readonly ILogger<AttachmentUpdateOrchestrator> _logger;
    private readonly MiniTicketingDbContext _db;

    public AttachmentUpdateOrchestrator(
        IAttachmentStagingService staging,
        ILogger<AttachmentUpdateOrchestrator> logger,
        MiniTicketingDbContext db)
    {
        _staging = staging;
        _logger = logger;
        _db = db;
    }

    public async Task ApplyAsync(Ticket ticket, AttachmentChangeSet changeSet, CancellationToken ct)
    {
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

    // ---------------- DELETE ----------------
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

            // storage
            await _staging.CopyAttachmentFilesAsync(original, deleted, ct);
            await _staging.RemoveAttachmentFilesAsync(original, ct);

            // DB – csak ha tényleg létezik
            var exists = await _db.TicketAttachments
                .AsNoTracking()
                .AnyAsync(a => a.Id == attachment.Id, ct);

            if (exists)
            {
                _db.Attach(attachment);
                _db.TicketAttachments.Remove(attachment);
            }
            else
            {
                _logger.LogWarning(
                    "Attachment {AttachmentId} wanted to be removed from ticket {TicketId}, but DB did not have it.",
                    attachment.Id,
                    ticket.Id);
            }

            deletedFiles.Add((original, deleted, attachment));
        }
    }

    // ---------------- ADD ----------------
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

            // 1) storage staging
            await _staging.AddAttachmentFilesAsync((attachment.Path!, file), ct);

            // 2) EZ A FONTOS: mondd meg EF-nek, hogy ÚJ
            // ha ezt nem teszed meg → UPDATE-et küld (amit most láttál)
            _db.TicketAttachments.Add(attachment);
            // vagy: _db.Entry(attachment).State = EntityState.Added;

            // 3) domain/navigáció
            ticket.TicketAttachments.Add(attachment);

            // 4) rollback infó
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

            try { await _staging.RemoveAttachmentFilesAsync(stagedPath, ct); }
            catch { /* best effort */ }

            ticket.TicketAttachments?.Remove(added.attachment);
            // ha Added állapotban volt, vegyük le
            var entry = _db.Entry(added.attachment);
            if (entry.State == EntityState.Added)
                entry.State = EntityState.Detached;
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
            catch { /* best effort */ }

            if (!ticket.TicketAttachments.Any(a => a.Id == deleted.attachment.Id))
            {
                ticket.TicketAttachments.Add(deleted.attachment);
            }

            if (_db.Entry(deleted.attachment).State == EntityState.Detached)
            {
                _db.TicketAttachments.Add(deleted.attachment);
            }
        }
    }
}
