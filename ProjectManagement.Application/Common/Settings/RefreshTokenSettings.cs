namespace ProjectManagement.Application.Common.Settings;

public sealed class RefreshTokenSettings
{
    public const string SectionName = "RefreshToken";


    public int ExpirationDays { get; set; } = 14;

    public int MaximumActiveTokensPerUser { get; set; } = 5;
}