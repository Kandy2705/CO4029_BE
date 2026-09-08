using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Application.Services;
using CO4029_BE.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CO4029_BE.Facades;

[ApiController]
[Route("api/v1")]
public class CampusLocationFacade : Controller
{
    private readonly CampusLocationService service;

    public CampusLocationFacade(CampusLocationService service)
    {
        this.service = service;
    }

    private async Task<ActionResult> Execute<T>(Func<string, Task<T>> action, string message)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await action(token);
            return Ok(ApiResponse<T>.Ok(result, message));
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object?>.Fail(ex.Message, "NOT_FOUND"));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object?>.Fail("Internal server error.", "INTERNAL_ERROR"));
        }
    }

    [HttpGet("buildings/{buildingId}/floors")]
    public Task<ActionResult> GetFloors(string buildingId) =>
        Execute(token => service.GetFloors(buildingId, token), "Lấy danh sách tầng thành công");

    [HttpPost("buildings/{buildingId}/floors")]
    public Task<ActionResult> CreateFloor(string buildingId, [FromBody] CreateFloorRequest request) =>
        Execute(token => service.CreateFloor(buildingId, request, token), "Tạo tầng thành công");

    [HttpGet("floors/{floorId}")]
    public Task<ActionResult> GetFloor(string floorId) =>
        Execute(token => service.GetFloor(floorId, token), "Lấy tầng thành công");

    [HttpPut("floors/{floorId}")]
    public Task<ActionResult> UpdateFloor(string floorId, [FromBody] UpdateFloorRequest request) =>
        Execute(token => service.UpdateFloor(floorId, request, token), "Cập nhật tầng thành công");

    [HttpDelete("floors/{floorId}")]
    public Task<ActionResult> DeleteFloor(string floorId) =>
        Execute(token => service.DeleteFloor(floorId, token), "Xóa tầng thành công");

    [HttpGet("floors/{floorId}/rooms")]
    public Task<ActionResult> GetRooms(string floorId) =>
        Execute(token => service.GetRooms(floorId, token), "Lấy danh sách phòng thành công");

    [HttpPost("floors/{floorId}/rooms")]
    public Task<ActionResult> CreateRoom(string floorId, [FromBody] CreateRoomRequest request) =>
        Execute(token => service.CreateRoom(floorId, request, token), "Tạo phòng thành công");

    [HttpGet("rooms/{roomId}")]
    public Task<ActionResult> GetRoom(string roomId) =>
        Execute(token => service.GetRoom(roomId, token), "Lấy phòng thành công");

    [HttpPut("rooms/{roomId}")]
    public Task<ActionResult> UpdateRoom(string roomId, [FromBody] UpdateRoomRequest request) =>
        Execute(token => service.UpdateRoom(roomId, request, token), "Cập nhật phòng thành công");

    [HttpDelete("rooms/{roomId}")]
    public Task<ActionResult> DeleteRoom(string roomId) =>
        Execute(token => service.DeleteRoom(roomId, token), "Xóa phòng thành công");
}
