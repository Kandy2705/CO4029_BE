using CO4029_BE.Application.Exceptions;
using CO4029_BE.Domain.Constants;
using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Utils;

public static class AuthorizeHelper
{
    public static bool AuthorizeForEmployee(User user)
    {
        return string.Equals(user.role, UserRoles.Employee, StringComparison.OrdinalIgnoreCase)
               || AuthorizeForAdmin(user);
    }

    public static bool AuthorizeForAdmin(User user)
    {
        return string.Equals(user.role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase);
    }

    public static void EnsureAdmin(User user)
    {
        if (!AuthorizeForAdmin(user))
        {
            throw new ForbiddenAccessException("Admin permission is required.");
        }
    }
}
