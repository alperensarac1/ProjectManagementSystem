namespace ProjectManagement.Application.DTOs.Users;



public sealed class ResetUserPasswordRequestDto
{
    public string NewPassword { get; set; } = string.Empty;
}