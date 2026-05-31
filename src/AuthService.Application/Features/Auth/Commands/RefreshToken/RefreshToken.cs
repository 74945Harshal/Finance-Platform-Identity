using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using AuthService.Application.Common;
using AuthService.Application.Interfaces.Repositories;
using AuthService.Application.Interfaces.Services;
using AuthService.Domain.Entities;
using AuthService.Application.Features.Auth.Commands.Login;

namespace AuthService.Application.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResult>;

    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenService jwtTokenService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var hash = HashHelper.Sha256(request.RefreshToken);
            var existing = await _refreshTokenRepository.GetByTokenHashAsync(hash);

            if (existing == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            // Reuse detection: Token has been revoked previously
            if (existing.RevokedAt != null)
            {
                // Revoke all tokens in the user's family tree to contain breach
                await _refreshTokenRepository.RevokeAllForUserAsync(existing.UserId);
                await _refreshTokenRepository.SaveChangesAsync();

                throw new UnauthorizedAccessException("Compromised session. Please log in again.");
            }

            if (existing.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Expired refresh token.");
            }

            if (existing.User == null || !existing.User.IsActive)
            {
                throw new UnauthorizedAccessException("User is inactive.");
            }

            // Revoke current token
            existing.RevokedAt = DateTime.UtcNow;

            // Generate new token pair
            var newAccessToken = _jwtTokenService.GenerateAccessToken(existing.User);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
            var newRefreshTokenHash = HashHelper.Sha256(newRefreshToken);

            // Link old token to the replacement
            existing.ReplacedByTokenHash = newRefreshTokenHash;

            var newRefreshTokenEntity = new AuthService.Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = existing.UserId,
                TokenHash = newRefreshTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            return new LoginResult(newAccessToken, newRefreshToken);
        }
    }
}
