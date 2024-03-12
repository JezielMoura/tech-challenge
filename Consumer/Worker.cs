using Azure.Identity;
using Azure.Messaging.ServiceBus;

namespace Consumer;

public class Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = new ServiceBusClient(configuration["SBHostName"], new DefaultAzureCredential());
        var processor = client.CreateProcessor(configuration["SBQueueName"]);

        using (var scope = serviceProvider.CreateScope())
        {
            var customerService = scope.ServiceProvider.GetRequiredService<CustomerService>();

            processor.ProcessMessageAsync += customerService.HandleCustomerCreated;
            processor.ProcessErrorAsync += customerService.HandleCustomerCreatedError;

            _logger.LogInformation("Start Queue processing for Azure Service Bus queue {QueueName}", configuration["SBQueueName"]);

            await processor.StartProcessingAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        _logger.LogInformation("Stopping Queue processing for Queue {QueueName}", configuration["SBQueueName"]);

        await processor.CloseAsync(stoppingToken);
    }
}
