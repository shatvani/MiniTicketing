namespace MiniTicketing.Application.Abstractions.Persistence;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream content, string objectName, string contentType, CancellationToken ct);
    Task DeleteAsync(string objectName, CancellationToken ct);
    Task<Stream> DownloadAsync(string objectName, CancellationToken ct);
    Task<bool> ExistsAsync(string objectName, CancellationToken ct);
    Task<IEnumerable<string>> ListAsync(string prefix, CancellationToken ct);
    Task CopyAsync(string sourceObjectName, string destinationObjectName, CancellationToken ct);

}
