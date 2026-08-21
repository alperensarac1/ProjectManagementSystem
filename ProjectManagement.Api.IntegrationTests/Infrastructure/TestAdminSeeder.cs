using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagement.Api.IntegrationTests.Models;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Enums;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;


public static class TestAdminSeeder
{

    private const string DefaultPassword =
        "TestAdminPassword123";

    public static async Task<TestAdminAccount> SeedAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            serviceProvider);

        await using var scope =
            serviceProvider.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var passwordHasher =
            scope.ServiceProvider
                .GetRequiredService<IPasswordHasher>();

        const string email =
            "integration.admin@projectmanagement.test";

        var existingAdmin =
            await dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    user => user.Email == email,
                    cancellationToken);

        if (existingAdmin is not null)
        {
            var accountChanged = false;

            if (existingAdmin.IsDeleted)
            {
                existingAdmin.IsDeleted = false;
                accountChanged = true;
            }

            if (!existingAdmin.IsActive)
            {
                existingAdmin.IsActive = true;
                accountChanged = true;
            }

            if (existingAdmin.Role != UserRole.Admin)
            {
                existingAdmin.Role = UserRole.Admin;

                existingAdmin.TokenVersion++;

                accountChanged = true;
            }

            existingAdmin.PasswordHash =
                passwordHasher.Hash(
                    DefaultPassword);

            existingAdmin.UpdatedAt =
                DateTime.UtcNow;

            accountChanged = true;

            if (accountChanged)
            {
                await dbContext.SaveChangesAsync(
                    cancellationToken);
            }

            return new TestAdminAccount(
                existingAdmin.Id,
                existingAdmin.FirstName,
                existingAdmin.LastName,
                existingAdmin.Email,
                DefaultPassword);
        }

        var admin = new User
        {
            FirstName = "Integration",
            LastName = "Admin",
            Email = email,

            PasswordHash =
                passwordHasher.Hash(
                    DefaultPassword),

            Role = UserRole.Admin,
            Department = "Test",
            IsActive = true,
            IsDeleted = false,
            TokenVersion = 0,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Users.AddAsync(
            admin,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return new TestAdminAccount(
            admin.Id,
            admin.FirstName,
            admin.LastName,
            admin.Email,
            DefaultPassword);
    }
}