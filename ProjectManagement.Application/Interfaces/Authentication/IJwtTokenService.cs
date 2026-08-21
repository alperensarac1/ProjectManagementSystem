using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Interfaces.Authentication;

public interface IJwtTokenService
{
    JwtTokenResult GenerateAccessToken(User user);
    
}