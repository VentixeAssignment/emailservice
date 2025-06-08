using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EmailFunction;

public class AccountEmailVerificationService
{
    private readonly ILogger<AccountEmailVerificationService> _logger;

    public AccountEmailVerificationService(ILogger<AccountEmailVerificationService> logger)
    {
        _logger = logger;
    }

    [Function("SendVerificationEmail")]
    public async Task Run(
        [ServiceBusTrigger("account-created", Connection = "ACS_ConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        



        await messageActions.CompleteMessageAsync(message);
    }
}
