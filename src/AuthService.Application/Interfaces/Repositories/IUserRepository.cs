using System;
using System.Threading.Tasks;
using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByIdAsync(Guid id);

        Task AddAsync(User user);

        Task SaveChangesAsync();
    }
}
