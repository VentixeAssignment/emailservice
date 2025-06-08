using System.ComponentModel.DataAnnotations;

namespace EmailFunction.Models;

public class SendVerificationEmailModel
{

    [Required]
    public string Email { get; set; } = null!;
}
