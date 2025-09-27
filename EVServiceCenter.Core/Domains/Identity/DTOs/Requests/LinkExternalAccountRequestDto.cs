namespace EVServiceCenter.Core.Domains.Identity.DTOs.Requests
{
    public class LinkExternalAccountRequestDto
    {
        public string Provider { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
