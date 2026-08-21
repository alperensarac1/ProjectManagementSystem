namespace ProjectManagement.Application.Common.Models;

public class PagedResult<T>
{
  
    public IReadOnlyCollection<T> Items { get; init; } =
        Array.Empty<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }


    public int TotalCount { get; init; }

    public int TotalPages =>
        PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Create(
        IReadOnlyCollection<T> items,
        int page,
        int pageSize,
        int totalCount)
    {
        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}