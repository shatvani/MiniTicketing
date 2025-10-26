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

    public Task DeleteAsync(string objectName, CancellationToken ct)
        => _client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_bucket).WithObject(objectName), ct);
}
