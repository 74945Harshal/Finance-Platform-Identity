using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();
    }
}
