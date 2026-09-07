using CO4029_BE.Application.Services;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CO4029_BE.Facades;

[ApiController]
[Route("api/v1/contacts")]
public class ContactManagementFacade: Controller
{
    private readonly QuestionService  _questionService;

    public ContactManagementFacade(QuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet("categories/")]
    public async Task<ActionResult<IEnumerable<CategoryReponse>>> GetCategories()
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.GetCategories(token);
            return Ok(ApiResponse<IEnumerable<CategoryReponse>>.Ok(result, message: "Lấy các danh mục thành công"));
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

    [HttpGet("categories/{categoryId}")]
    public async Task<ActionResult<CategoryReponse>> GetCategory(string categoryId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.GetCategory(categoryId, token);
            return Ok(ApiResponse<CategoryReponse>.Ok(result, message: "Lấy danh mục id thành công"));
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

    [HttpPost("categories/")]
    public async Task<ActionResult<CategoryReponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.CreateCategory(request, token);
            return Ok(ApiResponse<CategoryReponse>.Ok(result, message: "Tạo danh mục thành công"));
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

    [HttpPut("categories/{categoryId}")]
    public async Task<ActionResult<CategoryReponse>> UpdateCategory(string categoryId, [FromBody] CreateCategoryRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.UpdateCategory(request, categoryId, token);
            return Ok(ApiResponse<CategoryReponse>.Ok(result, "Cập nhật thành công danh mục"));
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

    [HttpDelete("categories/{categoryId}")]
    public async Task<ActionResult<bool>> DeleteCategory(string categoryId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.DeleteCategory(categoryId, token);
            return Ok(ApiResponse<bool>.Ok(result, message: "Xóa danh mục thành công"));
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

    [HttpGet("questions/")]
    public async Task<ActionResult<IEnumerable<QuestionReponse>>> GetQuestions()
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.GetQuestions(token);
            return Ok(ApiResponse<IEnumerable<QuestionReponse>>.Ok(result, message: "Lấy tất cả các câu hỏi thành công"));
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
    
    [HttpGet("questions/{questionId}")]
    public async Task<ActionResult<QuestionReponse>> GetQuestion(string questionId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.GetQuestion(questionId, token);
            return Ok(ApiResponse<QuestionReponse>.Ok(result, message: "Lấy câu hỏi theo id"));
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

    [HttpGet("questions/categories/{categoryId}")]
    public async Task<ActionResult<IEnumerable<QuestionReponse>>> GetQuestionsByCategory(string categoryId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.GetQuestionsByCategory(categoryId, token);
            return Ok(ApiResponse<IEnumerable<QuestionReponse>>.Ok(result, message: "Lấy câu hỏi theo danh mục"));
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

    [HttpPost("questions/")]
    public async Task<ActionResult<QuestionReponse>> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.CreateQuestion(request, token);
            return Ok(ApiResponse<QuestionReponse>.Ok(result, message: "Tạo câu hỏi thành công"));
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

    [HttpPut("questions/{questionId}")]
    public async Task<ActionResult<QuestionReponse>> UpdateQuestion(string questionId, [FromBody] CreateQuestionRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.UpdateQuestion(request, questionId, token);
            return Ok(ApiResponse<QuestionReponse>.Ok(result, message: "Cập nhật câu hỏi thành công"));
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

    [HttpDelete("questions/{questionId}")]
    public async Task<ActionResult<bool>> DeleteQuestion(string questionId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.DeleteQuestion(questionId, token);
            return Ok(ApiResponse<bool>.Ok(result, message: "Xóa thành công câu hỏi"));
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

    [HttpGet("questions/{questionId}/answers")]
    public async Task<ActionResult<IEnumerable<AnswerReponse>>> GetAnswersByQuestion(string questionId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.GetAnswersByQuestion(questionId, token);
            return Ok(ApiResponse<IEnumerable<AnswerReponse>>.Ok(
                result,
                message: "Lấy câu trả lời theo câu hỏi thành công"));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<object?>.Fail(
                "Yêu cầu xác thực hoặc quyền truy cập không hợp lệ.",
                "UNAUTHORIZED"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy câu hỏi.", "NOT_FOUND"));
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object?>.Fail("Đã xảy ra lỗi nội bộ.", "INTERNAL_ERROR"));
        }
    }

    [HttpGet("answers/")]
    public async Task<ActionResult<IEnumerable<AnswerReponse>>> GetAnswers()
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.GetAnswers(token);
            return Ok(ApiResponse<IEnumerable<AnswerReponse>>.Ok(result, message: "Lấy các câu trả lời thành công"));
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

    [HttpGet("answers/{answerId}")]
    public async Task<ActionResult<AnswerReponse>> GetAnswer(string answerId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.GetAnswer(answerId, token);
            return Ok(ApiResponse<AnswerReponse>.Ok(result, message: "Lấy câu trả theo id thành công"));
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

    [HttpGet("users/{userId}/answers")]
    public async Task<ActionResult<IEnumerable<AnswerReponse>>> GetAnswersByUser(string userId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.GetAnswersByUser(userId, token);
            return Ok(ApiResponse<IEnumerable<AnswerReponse>>.Ok(result, message: "Lấy câu trả lời theo id của người dùng"));
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

    [HttpPost("answers/")]
    public async Task<ActionResult<AnswerReponse>> CreateAnswer([FromBody] CreateAnswerRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.CreateAnswer(request, token);
            return Ok(ApiResponse<AnswerReponse>.Ok(result, message: "Tạo câu hỏi thành công"));
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

    [HttpPut("answers/{answerId}")]
    public async Task<ActionResult<AnswerReponse>> UpdateAnswer(string answerId, [FromBody] CreateAnswerRequest request)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.UpdateAnswer(request, answerId, token);
            return Ok(ApiResponse<AnswerReponse>.Ok(result, message: "Cập nhật câu trả lời thành công"));
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

    [HttpDelete("answers/{answerId}")]
    public async Task<ActionResult<bool>> DeleteAnswer(string answerId)
    {
        try
        {
            var token = await AccessToken.GetAccessToken(HttpContext);
            var result = await _questionService.DeleteAnswer(answerId, token);
            return Ok(ApiResponse<bool>.Ok(result, message: "Xóa câu trả lời thành công"));
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
