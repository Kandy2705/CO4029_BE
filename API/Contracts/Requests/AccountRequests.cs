namespace CO4029_BE.API.Contracts.Requests;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = default!;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}

/// <summary>Checks a code without consuming it. Purpose: "register" or "reset".</summary>
public class VerifyOtpRequest
{
    public string Email { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
    public string Purpose { get; set; } = default!;
}

public class RequestEmailChangeRequest
{
    public string NewEmail { get; set; } = default!;
}

public class ChangeEmailRequest
{
    public string NewEmail { get; set; } = default!;
    public string OtpCode { get; set; } = default!;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}
