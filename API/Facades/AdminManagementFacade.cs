using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Application.Exceptions;
using CO4029_BE.Application.Services;
using CO4029_BE.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CO4029_BE.Facades;

[ApiController]
[Route("api/v1/admin")]
public class AdminManagementFacade : Controller
{
    private readonly UserService _userService;
    private readonly HistoryService _historyService;
    private readonly DashboardService _dashboardService;
    private readonly ILogger<AdminManagementFacade> _logger;

    public AdminManagementFacade(
        UserService userService,
        HistoryService historyService,
        DashboardService dashboardService,
        ILogger<AdminManagementFacade> logger)
    {
        _userService = userService;
        _historyService = historyService;
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserReponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<UserReponse>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _userService.GetUsersForAdminAsync(
                token, page, pageSize, search, role, isActive);
            return Ok(ApiResponse<PagedResponse<UserReponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "get admin users");
        }
    }

    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<UserReponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserReponse>> GetUser([FromRoute] string userId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _userService.GetUserForAdminAsync(token, userId);
            return Ok(ApiResponse<UserReponse>.Ok(result));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "get admin user detail");
        }
    }

    [HttpPut("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<UserReponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserReponse>> UpdateUser(
        [FromRoute] string userId,
        [FromBody] AdminUpdateUserRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _userService.UpdateUserForAdminAsync(token, userId, request);
            return Ok(ApiResponse<UserReponse>.Ok(result, "Cập nhật người dùng thành công"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "update admin user");
        }
    }

    [HttpPatch("users/{userId}/status")]
    [ProducesResponseType(typeof(ApiResponse<UserReponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserReponse>> UpdateUserStatus(
        [FromRoute] string userId,
        [FromBody] UpdateUserStatusRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _userService.UpdateUserStatusForAdminAsync(token, userId, request);
            return Ok(ApiResponse<UserReponse>.Ok(result, "Cập nhật trạng thái người dùng thành công"));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "update admin user status");
        }
    }

    [HttpGet("chat/histories")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<HistoryReponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<HistoryReponse>>> GetHistories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _historyService.GetHistoriesForAdminAsync(
                token, page, pageSize, userId, fromDate, toDate);
            return Ok(ApiResponse<PagedResponse<HistoryReponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "get admin chat histories");
        }
    }

    [HttpGet("dashboard/summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardSummaryResponse>> GetDashboardSummary()
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _dashboardService.GetSummaryAsync(token);
            return Ok(ApiResponse<DashboardSummaryResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "get admin dashboard summary");
        }
    }

    private ActionResult HandleException(Exception exception, string operation)
    {
        switch (exception)
        {
            case ArgumentException:
                return BadRequest(ApiResponse<object?>.Fail(exception.Message, "BAD_REQUEST"));
            case UnauthorizedAccessException:
                return Unauthorized(ApiResponse<object?>.Fail(
                    "Yêu cầu xác thực không hợp lệ hoặc tài khoản đã bị vô hiệu hóa.",
                    "UNAUTHORIZED"));
            case ForbiddenAccessException:
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object?>.Fail("Bạn không có quyền Admin.", "FORBIDDEN"));
            case KeyNotFoundException:
                return NotFound(ApiResponse<object?>.Fail("Không tìm thấy tài nguyên.", "NOT_FOUND"));
            case InvalidOperationException:
                return Conflict(ApiResponse<object?>.Fail(
                    "Không thể thực hiện thao tác ở trạng thái hiện tại.",
                    "CONFLICT"));
            default:
                _logger.LogError(exception, "Unexpected error while attempting to {Operation}", operation);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object?>.Fail("Đã xảy ra lỗi nội bộ.", "INTERNAL_ERROR"));
        }
    }
}
