using Minio;
using Minio.Exceptions;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using Minio.DataModel.Args;

namespace Beauvoir.Services
{
    public class MinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;

        public MinioService(IConfiguration config)
        {
            var endpoint = config["Minio:Endpoint"];
            var accessKey = config["Minio:AccessKey"];
            var secretKey = config["Minio:SecretKey"];
            _bucketName = config["Minio:BucketName"];
            var useSSL = bool.Parse(config["Minio:UseSSL"]);

            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSSL)
                .Build();
        }

        public async Task UploadFileAsync(string objectName, Stream data, string contentType)
        {
            // Verificar si el bucket existe
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_bucketName);
            bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
            }

            // Subir el archivo
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(data)
                .WithObjectSize(data.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
        }

        public async Task<Stream> DownloadFileAsync(string objectName)
        {
            var ms = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(ms));

            await _minioClient.GetObjectAsync(getObjectArgs);
            ms.Position = 0;
            return ms;
        }
    }
}