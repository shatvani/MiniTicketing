namespace MiniTicketing.Application.Abstractions.Persistence;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream content, string objectName, string contentType, CancellationToken ct);
    Task DeleteAsync(string objectName, CancellationToken ct);
}
