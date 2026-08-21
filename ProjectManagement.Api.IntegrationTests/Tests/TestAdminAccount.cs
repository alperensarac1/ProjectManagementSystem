namespace ProjectManagement.Api.IntegrationTests.Models;

public sealed record TestAdminAccount(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    string Password);