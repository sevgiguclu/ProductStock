namespace ProductStock.Application.DTOs.Common;

// Sadece sayfalama tercihleri: ?page=2&pageSize=20
public class PaginationParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
