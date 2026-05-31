using System;
using System.Threading.Tasks;
using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);

        Task<RefreshToken?> GetByTokenHashAsync(string hash);

        Task RevokeAllForUserAsync(Guid userId);

        Task SaveChangesAsync();
    }
}
