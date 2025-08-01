using System;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Company.Function;

public class ChunkToQueueTimer
{
    private readonly ILogger _logger;

    public ChunkToQueueTimer(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ChunkToQueueTimer>();
    }

    [Function("ChunkToQueueTimer")]
    public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobClient = new BlobContainerClient(connectionString, "uploads");
            var blob = blobClient.GetBlobClient("data.txt");

            if (!await blob.ExistsAsync())
            {
                _logger.LogWarning("Blob 'data.txt' does not exist in 'uploads' container.");
                return;
            }

            using var stream = await blob.OpenReadAsync();
            using var reader = new StreamReader(stream);

            var queueClient = new QueueClient(connectionString, "silivannah-processing-queue");
            await queueClient.CreateIfNotExistsAsync();


            const int chunkSize = 500;
            char[] buffer = new char[chunkSize];
            int charsRead;

            while ((charsRead = await reader.ReadBlockAsync(buffer, 0, chunkSize)) > 0)
            {
                var chunk = new string(buffer, 0, charsRead);

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(chunk));
                    await queueClient.SendMessageAsync(base64);

                    _logger.LogInformation($"Queued chunk: {chunk[..Math.Min(chunk.Length, 30)]}...");
                }
            }
        }
    }
}