using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

var builder = Host.CreateApplicationBuilder(args);
var appPath = Directory.GetCurrentDirectory();
var sbHostName = builder.Configuration["SBHostName"];
var sbQueueName = builder.Configuration["SBQueueName"];

Console.WriteLine("********");
Console.WriteLine(sbHostName);
Console.WriteLine(sbQueueName);

builder.Services.AddDbContext<CustomerContext>(options => 
    options.UseSqlite($"Data Source={Path.Join(appPath, "db.db")}"));

builder.Services.AddScoped<CustomerService>();
builder.Services.AddHostedService<Worker>();

builder.Services.AddAzureClients(clientBuilder => {
    clientBuilder.UseCredential(new AzureCliCredential());
    clientBuilder.AddServiceBusClientWithNamespace("sb-nett.servicebus.windows.net");
    
    clientBuilder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) => 
        provider.GetRequiredService<ServiceBusClient>().CreateSender(sbQueueName)).WithName(sbQueueName);
});

var host = builder.Build();
host.Run();
