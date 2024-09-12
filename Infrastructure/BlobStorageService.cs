using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace Store.Infrastructure;

public class BlobStorageService
{
    private BlobServiceClient _blobServiceClient;
    private string _containerName = "web";

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
        }

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        Uri uri = new Uri(fileUrl);
        string blobName = Path.GetFileName(uri.LocalPath);

        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }
}