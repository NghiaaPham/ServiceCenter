namespace EVServiceCenter.Core.Domains.Shared.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public int? PreviousPage => HasPreviousPage ? Page - 1 : null;
        public int? NextPage => HasNextPage ? Page + 1 : null;

        public int StartItem => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;
        public int EndItem => TotalCount == 0 ? 0 : Math.Min(Page * PageSize, TotalCount);
    }
}
