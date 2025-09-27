namespace EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests
{
    public class CustomerTypeQueryDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "TypeName";
        public bool SortDesc { get; set; } = false;
        public bool IncludeStats { get; set; } = false;
    }
}
