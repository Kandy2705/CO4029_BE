using AgenticAR.Application.Services;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CO4029_BE.Facades;

[ApiController]
[Route("api/v1/chat")]
public class ChatManagementFacade : Controller
{
    private readonly HistoryService _historyService;
    private readonly ChatboxService _chatboxService;

    public ChatManagementFacade(HistoryService historyService
                              , ChatboxService chatboxService)
    {
        _historyService = historyService;
        _chatboxService = chatboxService;
    }

    [HttpPost("histories/")]
    public async Task<ActionResult<HistoryReponse>> CreateHistory([FromBody]CreateHistoryRequest createHistoryRequest)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result =  await _historyService.CreateHistory(createHistoryRequest, token);
            return Ok(ApiResponse<HistoryReponse>.Ok(result, message: "Tạo lịch sử trò chuyện thành công"));
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

    [HttpGet("histories/")]
    public async Task<ActionResult<IEnumerable<HistoryReponse>>> GetAllHistoryByUserId()
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _historyService.GetHistoryByUserId(token);
            return Ok(ApiResponse<IEnumerable<HistoryReponse>>.Ok(result, message: "Lấy thông tin các lịch sử thành công"));
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

    [HttpPost("chatboxes/")]
    public async Task<ActionResult<IEnumerable<ChatboxReponse>>> CreateChatbox(
        [FromBody] CreateChatboxRequest createChatboxRequest)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _chatboxService.CreateChatbox(token, createChatboxRequest);
            return Ok(ApiResponse<ChatbotApiResponse>.Ok(result));
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

    [HttpGet("chatboxes/")]
    public async Task<ActionResult<IEnumerable<ChatboxReponse>>> GetAllChatboxes()
    {
        try
        {
            var  token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _chatboxService.GetAllChatboxes(token);
            return Ok(ApiResponse<IEnumerable<ChatboxReponse>>.Ok(result, message: "Lấy tất cả các chat box thành công"));
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

    [HttpGet("chatboxes/{chatboxId}")]
    public async Task<ActionResult<ChatboxReponse>> GetChatbox([FromRoute] string chatboxId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _chatboxService.GetChatboxById(token, chatboxId);
            return Ok(ApiResponse<ChatboxReponse>.Ok(result, message: "Lấy chat box theo id thành công"));
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

    [HttpGet("chatboxes/history/{historyId}")]
    public async Task<ActionResult<IEnumerable<ChatboxReponse>>> GetChatboxesByHistory([FromRoute] string historyId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _chatboxService.GetAllChatboxesByHistory(token, historyId);
            return Ok(ApiResponse<IEnumerable<ChatboxReponse>>.Ok(result, message: "Lấy chat box theo lịch sử chat thành công"));
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

    [HttpDelete("chatboxes/{chatboxId}")]
    public async Task<ActionResult> DeleteChatbox([FromRoute] string chatboxId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _chatboxService.DeleteChatbox(token, chatboxId);
            return Ok(ApiResponse<bool>.Ok(result, message: "Xóa thành công chatbox"));
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

    [HttpDelete("histories/{historyId}")]
    public async Task<ActionResult> DeleteHistory([FromRoute] string historyId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _chatboxService.DeleteHistory(token, historyId);
            return Ok(ApiResponse<bool>.Ok(result, message: "Xóa thành công chat box theo id của history"));
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