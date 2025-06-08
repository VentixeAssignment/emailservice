namespace EmailFunction.Models;

public class VerificationResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }
}
