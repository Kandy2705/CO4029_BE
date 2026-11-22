using CO4029_BE.Application.Exceptions;
using CO4029_BE.Application.Services;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CO4029_BE.Facades;

[ApiController]
[Route("api/v1/users")]
public class UserManagementFacade : Controller
{
    private readonly UserService userService;
    private readonly AuthService authService;
    private readonly AccountService accountService;
    private readonly ILogger<UserManagementFacade> logger;

    public UserManagementFacade(UserService userService, AuthService authService, AccountService accountService,
        ILogger<UserManagementFacade> logger)
    {
        this.userService = userService;
        this.authService = authService;
        this.accountService = accountService;
        this.logger = logger;
    }

    // ---------- Registration ----------

    /// <summary>Sends a 4-digit code to an email that has no account yet (409 if it already has one).</summary>
    [HttpPost("register/request-otp")]
    public Task<ActionResult> RequestRegisterOtp([FromBody] SendOTPRequest request)
        => Execute(async () =>
        {
            await accountService.RequestRegisterOtpAsync(request.Email);
            return true;
        }, "Đã gửi mã OTP đăng ký");

    /// <summary>Checks a registration or reset code without consuming it (data = true/false).</summary>
    [HttpPost("verify-otp")]
    public Task<ActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        => Execute(() => accountService.VerifyOtpAsync(request), "Đã kiểm tra mã OTP");

    /// <summary>Creates a customer account; requires the code from register/request-otp.</summary>
    [HttpPost("create-customer")]
    public Task<ActionResult> RegisterCustomer([FromBody] CreateUserRequest request)
        => Execute(() => userService.RegisterCustomerAsync(request), "Tạo user thành công");

    // ---------- Session ----------

    [HttpPost("login")]
    public Task<ActionResult> Login([FromBody] LoginRequest request)
        => Execute(() => authService.LoginAsync(request), "Đăng nhập thành công");

    /// <summary>Exchanges a refresh token for a new access token.</summary>
    [HttpPost("refresh-token")]
    public Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        => Execute(() => accountService.RefreshAsync(request.RefreshToken), "Làm mới phiên đăng nhập thành công");

    [HttpGet("me")]
    public Task<ActionResult> GetMe()
        => ExecuteWithToken(token => authService.GetMeAsync(token), "Lấy thông tin bản thân thành công");

    [HttpPut("update-customer")]
    public Task<ActionResult> Update([FromBody] UpdateUserRequest request)
        => ExecuteWithToken(token => userService.UpdateUserAsync(request, token), "Cập nhật người dùng thành công");

    // ---------- Password ----------

    [HttpPost("request-password-change")]
    public Task<ActionResult> RequestPasswordChange([FromBody] SendOTPRequest request)
        => Execute(() => authService.SendOtpCode(request.Email), "Đã gửi OTP code");

    [HttpPost("change-password")]
    public Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        => Execute(() => authService.ChangePasswordAsync(
            request.email, request.otpCode, request.newPassword, request.oldPassword), "Đổi mật khẩu thành công");

    /// <summary>
    /// Starts a password reset. The response is the same whether or not the email has an account.
    /// </summary>
    [HttpPost("forgot-password")]
    public Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        => Execute(async () =>
        {
            await accountService.RequestPasswordResetAsync(request.Email);
            return true;
        }, "Nếu email đã đăng ký, mã OTP đã được gửi tới email đó");

    [HttpPost("reset-password")]
    public Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        => Execute(async () =>
        {
            await accountService.ResetPasswordAsync(request);
            return true;
        }, "Đặt lại mật khẩu thành công");

    // ---------- Email change ----------

    /// <summary>Sends a code to the new email address of the signed-in user.</summary>
    [HttpPost("request-email-change")]
    public Task<ActionResult> RequestEmailChange([FromBody] RequestEmailChangeRequest request)
        => ExecuteWithToken(async token =>
        {
            await accountService.RequestEmailChangeAsync(token, request.NewEmail);
            return true;
        }, "Đã gửi mã OTP tới email mới");

    [HttpPost("change-email")]
    public Task<ActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
        => ExecuteWithToken(token => accountService.ChangeEmailAsync(token, request), "Đổi email thành công");

    [HttpDelete("delete-user")]
    public Task<ActionResult> DeleteUser()
        => ExecuteWithToken(token => userService.DeleteUserAsync(token), "Xóa người dùng thành công");

    // ---------- Helpers ----------

    private async Task<ActionResult> ExecuteWithToken<T>(Func<string, Task<T>> action, string message)
        => await Execute(async () => await action(await AccessToken.GetAccessToken(HttpContext)), message);

    private async Task<ActionResult> Execute<T>(Func<Task<T>> action, string message)
    {
        try
        {
            var result = await action();
            return Ok(ApiResponse<T>.Ok(result, message));
        }
        catch (OtpCooldownException ex)
        {
            Response.Headers.RetryAfter = ex.RetryAfterSeconds.ToString();
            return StatusCode(StatusCodes.Status429TooManyRequests,
                ApiResponse<object?>.Fail(ex.Message, "TOO_MANY_REQUESTS"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message, "BAD_REQUEST"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object?>.Fail(ex.Message, "CONFLICT"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message, "UNAUTHORIZED"));
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object?>.Fail(ex.Message, "FORBIDDEN"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in {Path}", Request.Path);
            return StatusCode(500, ApiResponse<object?>.Fail("Lỗi hệ thống, vui lòng thử lại sau.", "INTERNAL_ERROR"));
        }
    }
}
