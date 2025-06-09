using Azure.Messaging.ServiceBus;
using EmailFunction.Models;
using EmailFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace EmailFunction;

public class AccountEmailVerificationService(ILogger<AccountEmailVerificationService> logger, IEmailVerificationService emailVerificationService)
{
    private readonly ILogger<AccountEmailVerificationService> _logger = logger;
    private readonly IEmailVerificationService _emailVerificationService = emailVerificationService;

    [Function("SendVerificationEmail")]
    public async Task Send(
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

    [Function("VerifyVerificationCode")]
    public async Task<HttpResponseData> Verify(
        [HttpTrigger(AuthorizationLevel.Function, "post")] 
        HttpRequestData request)
    {
        _logger.LogInformation("Message Body: {body}", request.Body);


        var model = await request.ReadFromJsonAsync<VerifyVerificationCodeModel>();

        if (model == null || string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
        {
            _logger.LogError($"Invalid or missing email address in request: {request.Body}");
            
            var response = request.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("Invalid or missing email address.");
            return response;
        }

        try
        {
            var result = _emailVerificationService.VerifyVerificationCode(model);
            
            var response = request.CreateResponse(result.Succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest);

            await response.WriteStringAsync(result.Succeeded ? "Verification successful." : result.ErrorMessage ?? "Verification failed.");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error verifying verification code for account: {request.Body}");
            var response = request.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync($"Failed to verify verification code. {request.Body}");
            return response;
        }
    }
}
