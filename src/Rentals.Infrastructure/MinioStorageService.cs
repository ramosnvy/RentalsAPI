using Minio;
using Minio.DataModel.Args;
using Rentals.Application.Abstractions;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucketName = "rentals-files";

    public MinioStorageService(IMinioClient client)
    {
        _client = client;
    }

    public async Task UploadAsync(string fileName, byte[] content)
    {
        using var stream = new MemoryStream(content);

        bool found = await _client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_bucketName));

        if (!found)
        {
            await _client.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_bucketName));
        }

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(fileName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType("image/png"));
    }
}