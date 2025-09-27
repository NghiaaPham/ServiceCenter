namespace EVServiceCenter.Core.Domains.Shared.Models
{
    public static class PagedResultFactory
    {
        public static PagedResult<T> Create<T>(
            IEnumerable<T> items,
            int totalCount,
            int page,
            int pageSize)
        {
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public static PagedResult<T> Empty<T>(int page = 1, int pageSize = 10)
        {
            return new PagedResult<T>
            {
                Items = new List<T>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize,
                TotalPages = 0
            };
        }
    }
}
