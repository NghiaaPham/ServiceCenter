namespace EVServiceCenter.Core.Domains.Identity.Interfaces
{
    /// <summary>
    /// Service qu?n l� JWT Token Blacklist
    /// D�ng cho logout v� revoke tokens
    /// </summary>
    public interface ITokenBlacklistService
    {
        /// <summary>
        /// Revoke (blacklist) m?t JWT token
        /// </summary>
        /// <param name="token">JWT token c?n revoke</param>
        /// <param name="userId">User ID c?a ng??i logout</param>
        /// <param name="reason">L� do revoke (Logout, PasswordChanged, SecurityBreach, etc.)</param>
        /// <param name="ipAddress">IP address khi revoke</param>
        /// <param name="userAgent">User Agent khi revoke</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True n?u revoke th�nh c�ng</returns>
        Task<bool> RevokeTokenAsync(
            string token,
            int userId,
            string reason,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ki?m tra xem token c� b? revoke (blacklist) kh�ng
        /// </summary>
        /// <param name="token">JWT token c?n check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True n?u token ?� b? revoke</returns>
        Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revoke t?t c? tokens c?a m?t user
        /// Use case: User ??i password, security breach, etc.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="reason">L� do revoke</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>S? l??ng tokens ?� revoke</returns>
        Task<int> RevokeAllUserTokensAsync(
            int userId,
            string reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleanup expired tokens kh?i blacklist
        /// Ch?y b?i background job ?? gi?m database size
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>S? l??ng tokens ?� cleanup</returns>
        Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get token expiry time t? JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Expiry DateTime, null n?u kh�ng parse ???c</returns>
        DateTime? GetTokenExpiry(string token);
    }
}
