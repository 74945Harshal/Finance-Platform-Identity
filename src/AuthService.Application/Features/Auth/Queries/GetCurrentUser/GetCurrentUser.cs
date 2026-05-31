using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AuthService.Application.Features.Auth.DTOs;
using AuthService.Application.Interfaces.Repositories;

namespace AuthService.Application.Features.Auth.Queries.GetCurrentUser
{
    public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;

    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
    {
        private readonly IUserRepository _userRepository;

        public GetCurrentUserQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null || !user.IsActive)
            {
                throw new KeyNotFoundException("User not found or is inactive.");
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
