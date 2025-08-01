using System;
using System.Text;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class DequeueChunk
{
    private readonly ILogger<DequeueChunk> _logger;

    public DequeueChunk(ILogger<DequeueChunk> logger)
    {
        _logger = logger;
    }

    [Function(nameof(DequeueChunk))]
    public async Task RunAsync([QueueTrigger("silivannah-processing-queue", Connection = "AzureWebJobsStorage")] QueueMessage message)
    {
        _logger.LogInformation("C# Queue trigger function processed: {messageText}", message.MessageText);
        try
        {

            _logger.LogInformation($"Dequeued message: {message.MessageText}");

            await Task.Delay(200);

            _logger.LogInformation("Message processed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message.");
            throw; // Let the runtime retry if needed
        }
    }
}