using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace Store.Infrastructure;

public class BlobStorageService
{
    private BlobServiceClient _blobServiceClient;
    private string _containerName = "iparts";

    public BlobStorageService(BlobServiceClient blob)
    {
        _blobServiceClient = blob;
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        string uniqueFileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        BlobClient blobClient = containerClient.GetBlobClient(uniqueFileName);

        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            stream.Close();
        }

        return blobClient.Uri.ToString();
    }

    public async Task<string> UploadFileFromPathAsync(string filePath)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        string uniqueFileName = filePath;
        BlobClient blobClient = containerClient.GetBlobClient(uniqueFileName);
        if (await blobClient.ExistsAsync() == false)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "application/x-yaml" });
                fileStream.Close();
            }
        }
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> GetFileStreamAsync(string fileName)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        BlobClient blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException($"File '{fileName}' does not exist in blob storage.");
        }

        BlobDownloadInfo download = await blobClient.DownloadAsync();
        return download.Content; // This is a stream
    }

    public async Task DeleteFileAsync(string fileName)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        BlobClient blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync();
    }
}