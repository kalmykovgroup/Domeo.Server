namespace Domeo.Shared.Contracts;

public sealed record PaginatedResponse<T>(
    int Total,
    int Page,
    int PageSize,
    List<T> Items);
