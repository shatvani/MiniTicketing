using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using MiniTicketing.Application.Abstractions.Persistence;
using MiniTicketing.Infrastructure.Options;

namespace MiniTicketing.Infrastructure.Persistence.Storage;
public sealed class MinioFileStorageService : IFileStorageService
{
    private readonly MinioClient _client;
    private readonly string _bucket;

    public MinioFileStorageService(IOptions<MinioOptions> options)
    {
        var opt = options.Value;
        _bucket = opt.BucketName;
        _client = (MinioClient)new MinioClient()
            .WithEndpoint(opt.Endpoint)
            .WithCredentials(opt.AccessKey, opt.SecretKey)
            .Build();
    }

    public async Task<string> UploadAsync(Stream content, string objectName, string contentType, CancellationToken ct)
    {
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType), ct);

        return objectName;
    }
    public async Task<Stream> DownloadAsync(string objectName, CancellationToken ct)
    {
        var memoryStream = new MemoryStream();
        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;
            }), ct);

        return memoryStream;
    }

    public async Task<bool> ExistsAsync(string objectName, CancellationToken ct)
    {
        try
        {
            await _client.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName), ct);
            return true;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return false;
        }
    }

    public async Task<IEnumerable<string>> ListAsync(string prefix, CancellationToken ct)
    {
        var objects = new List<string>();
        var observable = _client.ListObjectsAsync(new ListObjectsArgs()
            .WithBucket(_bucket)
            .WithPrefix(prefix)
            .WithRecursive(true), ct);

        var subscription = observable.Subscribe(
            item => objects.Add(item.Key),
            ex => throw ex);

        // Wait for the listing to complete
        await Task.Delay(1000, ct); // Simple delay to allow listing to complete

        subscription.Dispose();
        return objects;
    }

    public async Task CopyAsync(string sourceObjectName, string destinationObjectName, CancellationToken ct)
    {
        var cpSrcArgs = new CopySourceObjectArgs()
            .WithBucket(_bucket)
            .WithObject(sourceObjectName);
        await _client.CopyObjectAsync(new CopyObjectArgs()
            .WithBucket(_bucket)
            .WithObject(destinationObjectName)
            .WithCopyObjectSource(cpSrcArgs), ct);
    }

    public Task DeleteAsync(string objectName, CancellationToken ct)
        => _client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_bucket).WithObject(objectName), ct);
}
