using MailKit.Net.Smtp;
using MimeKit;
using System;

namespace CO4029_BE.Utils;

/// <summary>The Gmail account that sends OTP codes is not set up (App Settings GmailSender / GmailAppPassword).</summary>
public sealed class MailNotConfiguredException : IOException
{
    public MailNotConfiguredException()
        : base($"OTP mail is not configured: set {EmailService.SenderKey} and {EmailService.AppPasswordKey}.")
    {
    }
}

public class EmailService : CO4029_BE.Application.Abstractions.IOtpMailer
{
    public const string SenderKey = "GmailSender";
    public const string AppPasswordKey = "GmailAppPassword";

    private readonly string? _senderEmail;
    private readonly string? _appPassword;

    public EmailService(IConfiguration configuration, ILogger<EmailService>? logger = null)
    {
        _senderEmail = configuration[SenderKey];
        _appPassword = configuration[AppPasswordKey];
        if (!IsConfigured)
        {
            logger?.LogError("OTP mail is not configured: {Sender} / {Password} are missing, so registration, password reset and email change cannot send codes.",
                SenderKey, AppPasswordKey);
        }
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_senderEmail) && !string.IsNullOrWhiteSpace(_appPassword);

    public void SendOTPMail(string recipientMail, string otp)
    {
        // Without this check MailKit throws ArgumentNullException("address"), which the API would report as a bad request.
        if (!IsConfigured)
            throw new MailNotConfiguredException();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Agentic AR", _senderEmail));
        message.To.Add(new MailboxAddress("", recipientMail));
        message.Subject = "Your OTP Code";
        
        var body = new TextPart("html")
        {
            Text = $@"
                <html>
                    <head>
                        <style>
                            body {{
                                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                background-color: #f4f4f9;
                                color: #333;
                                margin: 0;
                                padding: 20px;
                            }}
                            .container {{
                                width: 100%;
                                max-width: 600px;
                                margin: 0 auto;
                                background-color: #ffffff;
                                padding: 30px;
                                border-radius: 12px;
                                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                                text-align: center;
                            }}
                            .header {{
                                font-size: 28px;
                                font-weight: bold;
                                color: #6200ea;
                                margin-bottom: 20px;
                            }}
                            .otp {{
                                font-size: 40px;
                                font-weight: bold;
                                color: #ffffff;
                                background-color: #4caf50;
                                padding: 20px;
                                border-radius: 10px;
                                margin: 20px 0;
                            }}
                            .footer {{
                                font-size: 14px;
                                color: #777;
                                margin-top: 30px;
                            }}
                            .button {{
                                background-color: #4caf50; /* Màu xanh lá nhạt */
                                color: white;
                                padding: 12px 30px;
                                text-decoration: none;
                                border-radius: 5px;
                                font-weight: bold;
                                margin-top: 25px;
                                display: inline-block;
                                text-transform: uppercase;
                                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); /* Hiệu ứng bóng nhẹ */
                                transition: background-color 0.3s ease, transform 0.3s ease;
                            }}
                            .button:hover {{
                                background-color: #388e3c; /* Màu tối hơn khi hover */
                                transform: translateY(-3px); /* Hiệu ứng di chuyển nhẹ lên */
                            }}
                            .button:active {{
                                background-color: #2c6f3f; /* Màu tối khi bấm */
                                transform: translateY(0); /* Không di chuyển khi nhấn */
                            }}
                            .content {{
                                background: linear-gradient(to right, #ff7e5f, #feb47b);
                                padding: 30px;
                                border-radius: 10px;
                                margin-bottom: 20px;
                            }}
                            .content p {{
                                font-size: 16px;
                                color: #333;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <p>Mã OTP của bạn</p>
                            </div>
                            <div class='content'>
                                <p>Chào bạn,</p>
                                <p>Đây là mã OTP của bạn để thực hiện xác minh tài khoản:</p>
                                <div class='otp'>
                                    {otp}
                                </div>
                                <p>Mã OTP này có hiệu lực trong 5 phút. Vui lòng nhập mã OTP để tiếp tục.</p>
                            </div>
                            <div class='footer'>
                                <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
                            </div>
                        </div>
                    </body>
                </html>"
        };


        
        message.Body = body;

        using (var client = new SmtpClient())
        {
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate(_senderEmail, _appPassword);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}