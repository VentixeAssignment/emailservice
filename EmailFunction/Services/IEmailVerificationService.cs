using EmailFunction.Models;

namespace EmailFunction.Services
{
    public interface IEmailVerificationService
    {
        void SaveVerificationCodeToCache(SaveVerificationCodeModel model);
        Task<VerificationResult> SendVerificationEmailAsync(SendVerificationEmailModel model);
        VerificationResult VerifyVerificationCode(VerifyVerificationCodeModel model);
    }
}