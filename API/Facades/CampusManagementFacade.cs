using CO4029_BE.Application.Services;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CO4029_BE.Facades;

[ApiController]
[Route("api/v1/buildings")]
public class CampusManagementFacade : Controller
{
    private readonly CampusService campusService;

    public CampusManagementFacade(CampusService campusService)
    {
        this.campusService = campusService;
    }

    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<BuildingReponse>>> GetBuildings()
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await campusService.GetAllBuildings(token);
            return Ok(ApiResponse<IEnumerable<BuildingReponse>>.Ok(result
                , "Lấy các tòa nhà thành công"));
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

    [HttpGet("{buildingId}")]
    public async Task<ActionResult<BuildingReponse>> GetBuilding(string buildingId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await campusService.GetBuildingById(buildingId, token);
            return Ok(ApiResponse<BuildingReponse>.Ok(result, message: "Lấy tòa nhà thành công"));
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

    [HttpPost("")]
    public async Task<ActionResult<BuildingReponse>> CreateBuilding([FromBody] CreateBuildingRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await campusService.CreateBuilding(request, token);
            return Ok(ApiResponse<BuildingReponse>.Ok(result, message: "Tạo tòa nhà thành công"));
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

    [HttpPut("{buildingId}")]
    public async Task<ActionResult<BuildingReponse>> UpdateBuilding([FromBody] UpdateBuildingRequest request, string buildingId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await campusService.UpdateBuilding(request, token, buildingId);
            return Ok(ApiResponse<BuildingReponse>.Ok(result, message: "Thay đổi thông tin tòa nhà thành công"));
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

    [HttpDelete("{buildingId}")]
    public async Task<ActionResult<bool>> DeleteBuilding(string buildingId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await campusService.DeleteBuilding(buildingId, token);
            return Ok(ApiResponse<bool>.Ok(result, message: "Xóa thành công tòa nhà"));
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

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<BuildingReponse>> GetBuildingByUserId(string userId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await campusService.GetBuildingsByUserId(token, userId);
            return Ok(ApiResponse<IEnumerable<BuildingReponse>>.Ok(result, message: "Lấy tòa nhà theo thông tin người add"));
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