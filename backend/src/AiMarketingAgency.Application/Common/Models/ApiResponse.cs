namespace AiMarketingAgency.Application.Common.Models;

public class ApiResponse<T>
{
    public T Data { get; set; } = default!;
    public bool Success { get; set; } = true;
    public string? Error { get; set; }

    public static ApiResponse<T> Ok(T data) => new() { Data = data, Success = true };
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
