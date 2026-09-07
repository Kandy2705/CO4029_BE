using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using Supabase.Gotrue;
using User = CO4029_BE.Domain.Entities.User;

namespace CO4029_BE.API.Contracts.Mappers;

public static class UserMapper
{
    public static UserReponse ToResponse(this User user)
        => new UserReponse
        {
            Id = user.id,
            Email = user.email,
            Name = user.name,
            Phone = user.phone,
            Birthday = user.birthday,
            Role = user.role,
            Gender = user.gender,
            IsActive = user.is_active
        };
    public static LoginResponse ToResponse(this Session session)
        => new LoginResponse
        {
            AccessToken = session.AccessToken
                          ?? throw new InvalidOperationException("Supabase did not return an access token."),
            RefreshToken = session.RefreshToken
                           ?? throw new InvalidOperationException("Supabase did not return a refresh token."),
            ExpiresAt = session.ExpiresAt(),
        }
;
}
