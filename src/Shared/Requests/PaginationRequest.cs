namespace IdentityService.Common.Requests
{
    // Incoming paging query. Bind with [AsParameters] to capture ?page=&pageSize=.
    // Values are clamped so callers can't ask for page 0 or an unbounded page size.
    public record PaginationRequest(int Page = 1, int PageSize = 20)
    {
        public int Page { get; init; } = Page < 1 ? 1 : Page;
        public int PageSize { get; init; } = PageSize < 1 ? 20 : PageSize > 100 ? 100 : PageSize;
        public int Skip => (Page - 1) * PageSize;
    }
}
