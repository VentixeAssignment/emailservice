using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace EmailFunction;

public class AccountEmailVerificationService(ILogger<AccountEmailVerificationService> logger, IConfiguration config, EmailClient emailClient)
{
    private readonly ILogger<AccountEmailVerificationService> _logger = logger;
    private readonly IConfiguration _config = config;
    private readonly EmailClient _emailClient = emailClient;


    [Function("SendVerificationEmail")]
    public async Task Send(
        [ServiceBusTrigger("account-created", Connection = "ACS_ConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        _logger.LogInformation($"Email contentt {message.Body.ToString()}");

        var payload = message.Body.ToObjectFromJson<VerifyVerificationCodeModel>();

        if (payload == null || string.IsNullOrWhiteSpace(payload.Email) || !payload.Email.Contains('@') || string.IsNullOrWhiteSpace(payload.Code))
        {
            _logger.LogError($"Invalid or missing email address or code in message.");
            return;
        }

        var subject = "Email Verification Code";
        var plainTextContent = $@"
                {payload.Code}
            ";
        var htmlContent = $@"
                {payload.Code}
            ";

        var emailMessage = new EmailMessage(
            senderAddress: _config["SenderAddress"],
            recipients: new EmailRecipients([new(payload.Email)]),
            content: new EmailContent(subject)
            {
                PlainText = plainTextContent,
                Html = htmlContent
            }
        );
        _logger.LogInformation($"Sending email from: {emailMessage.SenderAddress}");
        _logger.LogInformation($"To: {string.Join(",", emailMessage.Recipients.To.Select(r => r.Address))}");
        _logger.LogInformation($"Subject: {emailMessage.Content.Subject}");
        _logger.LogInformation($"Plain text body: {emailMessage.Content.PlainText}");
        _logger.LogInformation($"HTML body: {emailMessage.Content.Html}");
        try
        {
            await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);
            await messageActions.CompleteMessageAsync(message);
            _logger.LogInformation($"Email was sent successfully to email {payload.Email}");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email for message with ID: {message.MessageId}");
            await messageActions.DeadLetterMessageAsync(message);
        }
    }    
}
