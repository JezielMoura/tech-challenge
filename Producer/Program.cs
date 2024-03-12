using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Producer;

var builder = WebApplication.CreateBuilder(args);

var sbHostName = builder.Configuration["SBHostName"];
var sbQueueName = builder.Configuration["SBQueueName"];

builder.Services.AddAzureClients(clientBuilder => {
    clientBuilder.UseCredential(new AzureCliCredential());
    clientBuilder.AddServiceBusClientWithNamespace(sbHostName);
    
    clientBuilder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) => 
        provider.GetRequiredService<ServiceBusClient>().CreateSender(sbQueueName)).WithName(sbQueueName);
});

var app = builder.Build();

app.MapPost("/", async (IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory, Customer customer) => {
    var sender = serviceBusSenderFactory.CreateClient(sbQueueName);
    var body = JsonSerializer.Serialize(customer);
    var message = new ServiceBusMessage(body);

    await sender.SendMessageAsync(message);

    return Results.Accepted();
});

app.Run();


namespace Producer
{
    public record Customer (string Name, string Address)
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    };
}