using MiniTicketing.Application.Features.Tickets;

namespace MiniTicketing.Application.Abstractions.Services;

public interface IAttachmentStagingService
{
  Task AddAttachmentFilesAsync((string, FileUploadDto) attachment, CancellationToken ct);

  Task RemoveAttachmentFilesAsync(string objectName, CancellationToken ct);

  Task CopyAttachmentFilesAsync(string srcName, string destName, CancellationToken ct);
}