using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Consumer;

public class CustomerService (CustomerContext customerContext, ILogger<CustomerService> logger, IConfiguration configuration)
{
    public async Task HandleCustomerCreated(ProcessMessageEventArgs arg)
    {
        var customer = JsonSerializer.Deserialize<Customer>(arg.Message.Body)!;

        await customerContext.AddAsync(customer);
        await customerContext.SaveChangesAsync();
        
       logger.LogInformation("Process finished for Queue {}, object {}", configuration["SBQueueName"], arg.Message.Body);
    }

    public Task HandleCustomerCreatedError(ProcessErrorEventArgs arg)
    {
        logger.LogError("Error while processing message {EventSource}: {Error}", arg.ErrorSource, arg.Exception!.ToString());
        return Task.CompletedTask;
    }
}