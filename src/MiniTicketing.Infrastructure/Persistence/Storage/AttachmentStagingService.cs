using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Application.Abstractions.Services;
using MiniTicketing.Application.Features.Tickets;

namespace MiniTicketing.Infrastructure.Persistence.Storage;

public class AttachmentStagingService : IAttachmentStagingService
{
  private readonly IFileStorageService _fileStorage;
  public AttachmentStagingService(IFileStorageService fileStorage)
  {
    _fileStorage = fileStorage;
  }
  
  public async Task AddAttachmentFilesAsync((string, FileUploadDto) attachment, CancellationToken ct)
  {
    await _fileStorage.UploadAsync(new MemoryStream(attachment.Item2.Content), $"added/{attachment.Item1}", attachment.Item2.ContentType, ct);
  }

  public async Task RemoveAttachmentFilesAsync(string objectName, CancellationToken ct)
  {
    await _fileStorage.DeleteAsync(objectName, ct);
  }

  public async Task CopyAttachmentFilesAsync(string srcName, string destName, CancellationToken ct)
  {
    await _fileStorage.CopyAsync(srcName, destName, ct);
  }
}