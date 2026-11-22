namespace CO4029_BE.Application.Abstractions;

/// <summary>Delivers a one-time code to an email address.</summary>
public interface IOtpMailer
{
    void SendOTPMail(string recipientMail, string otp);
}
