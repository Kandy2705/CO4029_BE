using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Application.Services;
using CO4029_BE.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CO4029_BE.Facades;

/// <summary>Read-only campus data for the mobile app (any active, signed-in user).</summary>
[ApiController]
[Route("api/v1/campus")]
public class CampusMapFacade : Controller
{
    private readonly CampusMapService service;
    private readonly ILogger<CampusMapFacade> logger;

    public CampusMapFacade(CampusMapService service, ILogger<CampusMapFacade> logger)
    {
        this.service = service;
        this.logger = logger;
    }

    [HttpGet("map")]
    public async Task<ActionResult> GetMap()
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await service.GetMapAsync(token);
            return Ok(ApiResponse<CampusMapResponse>.Ok(result, "Lấy dữ liệu khuôn viên thành công"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message, "UNAUTHORIZED"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load campus map");
            return StatusCode(500, ApiResponse<object?>.Fail("Lỗi hệ thống, vui lòng thử lại sau.", "INTERNAL_ERROR"));
        }
    }
}
