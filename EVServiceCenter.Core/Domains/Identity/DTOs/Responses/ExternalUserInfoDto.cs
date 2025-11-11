namespace EVServiceCenter.Core.Domains.Identity.DTOs.Responses
{
    public class ExternalUserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public string Provider { get; set; } = string.Empty;

        // Additional fields from external providers
        public string? PhoneNumber { get; set; }
        public DateOnly? Birthday { get; set; }
        public string? Gender { get; set; }
        public string? Location { get; set; }
    }
}
