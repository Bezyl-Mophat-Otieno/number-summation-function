using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Company.Function;

public class ImageResizer
{
    private readonly ILogger<ImageResizer> _logger;

    public ImageResizer(ILogger<ImageResizer> logger)
    {
        _logger = logger;
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