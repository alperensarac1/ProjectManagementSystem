using ProjectManagement.Infrastructure.Data;
using ProjectManagement.Infrastructure.Initialization;

namespace ProjectManagement.Api.IntegrationTests.Infrastructure;


public sealed class TestDatabaseInitializer
    : IDatabaseInitializer
{
    private readonly ApplicationDbContext _dbContext;

    public TestDatabaseInitializer(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(
            cancellationToken);
    }
}