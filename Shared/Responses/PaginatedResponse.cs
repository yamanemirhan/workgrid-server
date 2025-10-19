namespace Shared.Responses;

public class PaginatedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }

    public PaginatedResponse()
    {
    }

    public PaginatedResponse(IEnumerable<T> data, int currentPage, int pageSize, int totalCount, string message)
    {
        Success = true;
        Data = data;
        Message = message;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        HasPrevious = currentPage > 1;
        HasNext = currentPage < TotalPages;
    }
}
