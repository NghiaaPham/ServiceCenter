namespace EVServiceCenter.Core.Domains.Identity.Entities
{
    /// <summary>
    /// Entity l?u tr? JWT tokens ?ã b? revoke (logout/invalidated)
    /// Dùng cho Token Blacklist pattern
    /// </summary>
    public class RevokedToken
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// JWT Token ?ã b? revoke (unique)
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// User ID c?a ng??i logout
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Lý do revoke: Logout, PasswordChanged, SecurityBreach, etc.
        /// </summary>
        public string? RevokeReason { get; set; }

        /// <summary>
        /// Th?i ?i?m token b? revoke
        /// </summary>
        public DateTime RevokedAt { get; set; }

        /// <summary>
        /// Th?i ?i?m token expire (?? cleanup)
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// IP address khi logout
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent khi logout
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Navigation property to User
        /// </summary>
        public User? User { get; set; }
    }
}
