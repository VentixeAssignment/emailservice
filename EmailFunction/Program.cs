using Azure.Communication.Email;
using EmailFunction;
using EmailFunction.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton(new EmailClient(builder.Configuration["CommunicationService"]));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddTransient<AccountEmailVerificationService>();

builder.Build().Run();
