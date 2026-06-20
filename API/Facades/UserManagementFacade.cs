using CO4029_BE.Application.Services;
using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace CO4029_BE.Facades;

[ApiController]
[Route("api/v1/users")]
public class UserManagementFacade : Controller
{
    private readonly UserService userService;
    private readonly AuthService authService;

    public UserManagementFacade(UserService userService, AuthService authService)
    {
        this.userService = userService;
        this.authService = authService;
    }

    [HttpPost("create-customer")]
    public async Task<ActionResult<UserReponse>> RegisterCustomer([FromBody] CreateUserRequest request)
    {
        try
        {
            var result = await userService.RegisterCustomerAsync(request);
            return Ok(ApiResponse<UserReponse>.Ok(result, message: "Tạo user thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object?>.Fail(ex.Message, "CONFLICT"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message, "UNAUTHORIZED"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object?>.Fail(ex.Message, "INTERNAL_ERROR"));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await authService.LoginAsync(request);
            return Ok(ApiResponse<LoginResponse>.Ok(result, "Đăng nhập thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object?>.Fail(ex.Message, "CONFLICT"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message, "UNAUTHORIZED"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object?>.Fail(ex.Message, "INTERNAL_ERROR"));
        }
    }
    
    [HttpGet("me")]
    public async Task<ActionResult<UserReponse>> GetMe()
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);

            var result = await authService.GetMeAsync(token);
            return Ok(ApiResponse<UserReponse>.Ok(result, message: "Lấy thông tin bản thân thành công"));

        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object?>.Fail(ex.Message, "CONFLICT"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message, "UNAUTHORIZED"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object?>.Fail(ex.Message, "INTERNAL_ERROR"));
        }
    }

    [HttpPut("update-customer")]
    public async Task<ActionResult<UserReponse>> Update([FromBody] UpdateUserRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            
            var result = await userService.UpdateUserAsync(request, token);
            return Ok(ApiResponse<UserReponse>.Ok(result,"Cập nhật người dùng thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object?>.Fail(ex.Message, "CONFLICT"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message, "UNAUTHORIZED"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object?>.Fail(ex.Message, "INTERNAL_ERROR"));
        }
    }

    [HttpPost("request-password-change")]
    public async Task<ActionResult> RequestPasswordChange([FromBody] SendOTPRequest request)
    {
        try
        {
            var email = request.Email;
            bool result = await authService.SendOtpCode(email);
            return Ok(ApiResponse<bool>.Ok(result, message: "Đã gửi OTP code"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object?>.Fail(ex.Message, "CONFLICT"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message, "UNAUTHORIZED"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object?>.Fail(ex.Message, "INTERNAL_ERROR"));
        }
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<UserReponse>> ChangePassword([FromBody]  ChangePasswordRequest request)
    {
        try
        {
            var result = await authService.ChangePasswordAsync(request.email,request.otpCode, request.newPassword, request.oldPassword);
            return Ok(ApiResponse<UserReponse>.Ok(result, message: "Đổi mật khẩu thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object?>.Fail(ex.Message, "CONFLICT"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message, "UNAUTHORIZED"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object?>.Fail(ex.Message, "INTERNAL_ERROR"));
        }
    }

    [HttpDelete("delete-user")]
    public async Task<ActionResult> DeleteUser()
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await userService.DeleteUserAsync(token);
            return Ok(ApiResponse<bool>.Ok(result, message: "Xóa người dùng thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object?>.Fail(ex.Message, "CONFLICT"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message, "UNAUTHORIZED"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object?>.Fail(ex.Message, "INTERNAL_ERROR"));
        }
    }
}