namespace ProjectManagement.Application.Interfaces.Storage;

/// <summary>
/// Yerel depolamadan açılan fiziksel dosya stream'ini
/// temsil eder.
/// </summary>
public sealed class StoredMailboxFileStream
{
    public Stream Content { get; init; } =
        Stream.Null;

    public long Length { get; init; }
}