using Azure.Messaging.ServiceBus;
using EmailFunction.Models;
using EmailFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EmailFunction;

public class AccountEmailVerificationService(ILogger<AccountEmailVerificationService> logger, IEmailVerificationService emailVerificationService)
{
    private readonly ILogger<AccountEmailVerificationService> _logger = logger;
    private readonly IEmailVerificationService _emailVerificationService = emailVerificationService;

    [Function("SendVerificationEmail")]
    public async Task Run(
        [ServiceBusTrigger("account-created", Connection = "ACS_ConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        
        var model = message.Body.ToObjectFromJson<SendVerificationEmailModel>();

        if(model == null || string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
        {
            _logger.LogError($"Invalid or missing email address in message: {message.Body}");
            return;
        }

        try
        {
            await _emailVerificationService.SendVerificationEmailAsync(model);
            await messageActions.CompleteMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email for message with ID: {message.MessageId}");
            await messageActions.DeadLetterMessageAsync(message);
        }
    }
}
