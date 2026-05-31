using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AuthService.Application.Common;
using AuthService.Application.Interfaces.Repositories;

namespace AuthService.Application.Features.Auth.Commands.Logout
{
    public record LogoutCommand(string RefreshToken) : IRequest<Unit>;

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return Unit.Value;
            }

            var hash = HashHelper.Sha256(request.RefreshToken);
            var existing = await _refreshTokenRepository.GetByTokenHashAsync(hash);

            if (existing != null && existing.RevokedAt == null)
            {
                existing.RevokedAt = DateTime.UtcNow;
                await _refreshTokenRepository.SaveChangesAsync();
            }

            return Unit.Value;
        }
    }
}
