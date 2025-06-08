using Azure;
using Azure.Communication.Email;
using EmailFunction.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace EmailFunction.Services;

public class EmailVerificationService(IConfiguration config, EmailClient client, IMemoryCache cache) : IEmailVerificationService
{
    private readonly IConfiguration _config = config;
    private readonly EmailClient _client = client;
    private readonly IMemoryCache _cache = cache;
    private static readonly Random _random = new();


    public async Task<VerificationResult> SendVerificationEmailAsync(SendVerificationEmailModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains('@'))
            {
                return new VerificationResult { Succeeded = false, ErrorMessage = "Invalid or missing email address." };
            }

            var code = _random.Next(100000, 999999).ToString();
            var subject = "Email Verification Code";
            var plainTextContent = $@"
                {code}
            ";
            var htmlContent = $@"
                {code}
            ";

            var emailMessage = new EmailMessage(
                senderAddress: _config["SenderAddress"],
                recipients: new EmailRecipients([new(model.Email)]),
                content: new EmailContent(subject)
                {
                    PlainText = plainTextContent,
                    Html = htmlContent
                }
            );

            var sendOperation = await _client.SendAsync(WaitUntil.Started, emailMessage);

            SaveVerificationCodeToCache(new SaveVerificationCodeModel { Email = model.Email, VerificationCode = code, ValidFor = TimeSpan.FromMinutes(10) });

            return new VerificationResult { Succeeded = true, Message = "Verification email sent successfully." };
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error sending verification email: {ex}");
            return new VerificationResult { Succeeded = false, ErrorMessage = $"Failed to send verification email: {ex}" };
        }
    }

    public void SaveVerificationCodeToCache(SaveVerificationCodeModel model)
    {
        _cache.Set(model.Email.ToLower(), model.VerificationCode, model.ValidFor);
    }

    public VerificationResult VerifyVerificationCode(VerifyVerificationCodeModel model)
    {
        var key = model.Email.ToLower();

        if (_cache.TryGetValue(key, out string? cachedCode))
        {
            if(cachedCode == model.VerificationCode)
            {
                _cache.Remove(key);
                return new VerificationResult { Succeeded = true, Message = "Verification code is valid." };
            }
            else
            {
                return new VerificationResult { Succeeded = false, ErrorMessage = "Invalid verification code." };
            }
        }
        return new VerificationResult { Succeeded = false, ErrorMessage = "Invalid key." };
    }
}
