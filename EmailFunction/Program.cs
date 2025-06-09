using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailFunction;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Services.AddSingleton<ServiceBusSender>(x =>
{
    var client = new ServiceBusClient(builder.Configuration["ServiceBusConnection"]);
    return client.CreateSender("account-created");
});

builder.Services.AddSingleton(new EmailClient(builder.Configuration["CommunicationService"]));

builder.Services.AddMemoryCache();
builder.Services.AddTransient<AccountEmailVerificationService>();

builder.Build().Run();
