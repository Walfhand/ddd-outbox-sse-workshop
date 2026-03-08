namespace Engine.Web;

public record FilterRequest(int PageIndex = 1, int PageSize = 20, string? SortBy = null, bool SortAscending = true)
{
}