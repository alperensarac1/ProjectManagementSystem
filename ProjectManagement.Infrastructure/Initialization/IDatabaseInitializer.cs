namespace ProjectManagement.Infrastructure.Initialization;

public interface IDatabaseInitializer
{
    Task InitializeAsync(
        CancellationToken cancellationToken = default);
}