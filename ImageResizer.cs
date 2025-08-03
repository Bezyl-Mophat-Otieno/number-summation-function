using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Company.Function.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Company.Function;

public class ImageResizer
{
    private readonly ILogger<ImageResizer> _logger;
    private readonly KeyVaultService _keyVault;

    public ImageResizer(ILogger<ImageResizer> logger, KeyVaultService keyVault)
    {
        _logger = logger;
        _keyVault = keyVault;
    }

    [Function(nameof(ImageResizer))]
    public async Task Run([BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name)
    {
        
        if (!name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
        !name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) &&
        !name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation($"Skipping non-image file: {name}");
            return;
        }
        _logger.LogInformation($"Image uploaded: {name}");

        string dbAdmin = await _keyVault.GetSecretAsync("silivannah-sql-admin");
        string dbConnection = await _keyVault.GetSecretAsync("sillivannahdb-connection-string");

        _logger.LogInformation($"Silivannah Db  ConnectionString: {dbConnection}");
        _logger.LogInformation($"Silivannah Db Admin: {dbAdmin}");

        using var image = await Image.LoadAsync(stream);
        image.Mutate(x => x.Resize(200, 0)); // Resize width to 200px, keep aspect

        using var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream);
        outputStream.Position = 0;

        // Upload to "thumbnails"
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var blobClient = new BlobContainerClient(connectionString, "thumbnails");
        await blobClient.CreateIfNotExistsAsync();

        var blob = blobClient.GetBlobClient(name);
        await blob.UploadAsync(outputStream, overwrite: true);

        _logger.LogInformation($"Thumbnail saved to: thumbnails/{name}");
    }
}