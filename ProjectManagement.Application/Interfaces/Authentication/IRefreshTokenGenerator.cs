namespace ProjectManagement.Application.Interfaces.Authentication;


public interface IRefreshTokenGenerator
{
    string GenerateToken();

    string ComputeHash(string token);
}