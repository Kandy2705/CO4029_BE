namespace CO4029_BE.API.Contracts.Responses;

public class PagedResponse<T>
{
    public required IEnumerable<T> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public static PagedResponse<T> Create(
        IEnumerable<T> items,
        int page,
        int pageSize,
        int totalItems)
        => new()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0
                ? 0
                : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
}
