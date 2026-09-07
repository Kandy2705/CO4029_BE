namespace CO4029_BE.API.Contracts.Requests;

public class AdminUpdateUserRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Gender { get; set; }
    public string? Role { get; set; }
}
