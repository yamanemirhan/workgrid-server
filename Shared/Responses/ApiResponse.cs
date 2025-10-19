namespace Shared.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public ApiResponse()
    {
    }

    public ApiResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public ApiResponse(T data, string message)
    {
        Success = true;
        Data = data;
        Message = message;
    }

    public ApiResponse(bool success, string message, List<string> errors)
    {
        Success = success;
        Message = message;
        Errors = errors;
    }
}

// Non-generic version for responses without data
public class ApiResponse : ApiResponse<object>
{
    public ApiResponse() : base()
    {
    }

    public ApiResponse(bool success, string message) : base(success, message)
    {
    }

    public ApiResponse(bool success, string message, List<string> errors) : base(success, message, errors)
    {
    }
}
