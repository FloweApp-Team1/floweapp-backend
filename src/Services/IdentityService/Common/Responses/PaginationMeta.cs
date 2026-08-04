namespace IdentityService.Common.Responses
{
    // The "pagination" block returned alongside a paged list. The list items go in
    // "data"; this carries only the paging metadata.
    public record PaginationMeta
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public bool HasNextPage { get; init; }
        public bool HasPreviousPage { get; init; }

        public static PaginationMeta Create(int totalCount, int page, int pageSize)
        {
            var totalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
            return new()
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1 && page <= totalPages
            };
        }
    }
}
