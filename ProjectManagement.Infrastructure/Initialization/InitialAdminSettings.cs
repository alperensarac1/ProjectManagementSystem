namespace ProjectManagement.Infrastructure.Initialization;


public sealed class InitialAdminSettings
{

    public const string SectionName = "InitialAdmin";


    public bool Enabled { get; set; } = true;


    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public string? Department { get; set; }
}