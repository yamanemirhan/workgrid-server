namespace Shared.Responses;

public static class ResponseHelper
{
    // Success responses
    public static ApiResponse<T> Success<T>(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T>(data, message);
    }

    public static ApiResponse Success(string message = "Operation completed successfully")
    {
        return new ApiResponse(true, message);
    }

    // Error responses
    public static ApiResponse<T> Error<T>(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>(false, message, errors ?? new List<string>());
    }

    public static ApiResponse Error(string message, List<string>? errors = null)
    {
        return new ApiResponse(false, message, errors ?? new List<string>());
    }

    // Validation error
    public static ApiResponse ValidationError(List<string> errors)
    {
        return new ApiResponse(false, "Validation failed", errors);
    }

    // Not found
    public static ApiResponse<T> NotFound<T>(string message = "Resource not found")
    {
        return new ApiResponse<T>(false, message);
    }

    public static ApiResponse NotFound(string message = "Resource not found")
    {
        return new ApiResponse(false, message);
    }

    // Unauthorized
    public static ApiResponse<T> Unauthorized<T>(string message = "Unauthorized access")
    {
        return new ApiResponse<T>(false, message);
    }

    public static ApiResponse Unauthorized(string message = "Unauthorized access")
    {
        return new ApiResponse(false, message);
    }

    // Paginated response
    public static PaginatedResponse<T> Paginated<T>(IEnumerable<T> data, int currentPage, int pageSize, int totalCount, string message = "Data retrieved successfully")
    {
        return new PaginatedResponse<T>(data, currentPage, pageSize, totalCount, message);
    }
}