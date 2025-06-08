using System.ComponentModel.DataAnnotations;

namespace EmailFunction.Models;

public class VerifyVerificationCodeModel
{
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string VerificationCode { get; set; } = null!;
}
