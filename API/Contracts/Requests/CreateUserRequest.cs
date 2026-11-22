namespace CO4029_BE.API.Contracts.Requests;

public class CreateUserRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string? Name { get; set; }
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Gender { get; set; }

    /// <summary>Code sent by POST /users/register/request-otp; proves the email belongs to the registrant.</summary>
    public string? OtpCode { get; set; }
}
