namespace CO4029_BE.Domain.Constants;

public static class UserRoles
{
    public const string Customer = "Customer";
    public const string Employee = "Employee";
    public const string Admin = "Admin";

    public static bool IsValid(string? role)
        => role != null && All.Any(candidate =>
            string.Equals(candidate, role, StringComparison.OrdinalIgnoreCase));

    public static string Normalize(string role)
        => All.First(candidate =>
            string.Equals(candidate, role, StringComparison.OrdinalIgnoreCase));

    private static readonly string[] All = [Customer, Employee, Admin];
}
