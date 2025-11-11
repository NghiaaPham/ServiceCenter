using System.IdentityModel.Tokens.Jwt;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace EVServiceCenter.Infrastructure.Domains.Identity.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly EVDbContext _context;
        private readonly ILogger<TokenBlacklistService> _logger;
        private readonly IMemoryCache _cache;

        // Cache key prefix
        private const string CachePrefix = "revoked_token_";

        public TokenBlacklistService(
            EVDbContext context,
            ILogger<TokenBlacklistService> logger,
            IMemoryCache cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<bool> RevokeTokenAsync(
            string token,
            int userId,
            string reason,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Attempted to revoke null/empty token");
                    return false;
                }

                // Check if token already revoked
                var existingRevoked = await _context.RevokedTokens
                    .AnyAsync(rt => rt.Token == token, cancellationToken);

                if (existingRevoked)
                {
                    _logger.LogInformation("Token already revoked for user {UserId}", userId);
                    return true; // Already revoked
                }

                // Get token expiry
                var expiresAt = GetTokenExpiry(token) ?? DateTime.UtcNow.AddHours(1); // Default 1h if can't parse

                // Create revoked token entry
                var revokedToken = new RevokedToken
                {
                    Token = token,
                    UserId = userId,
                    RevokeReason = reason,
                    RevokedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                _context.RevokedTokens.Add(revokedToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Remove any cached value for this token so subsequent checks hit DB (or will be repopulated)
                _cache.Remove(CachePrefix + token);

                _logger.LogInformation(
                    "Token revoked for user {UserId}. Reason: {Reason}, IP: {IP}",
                    userId, reason, ipAddress);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsTokenRevokedAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                var cacheKey = CachePrefix + token;

                if (_cache.TryGetValue(cacheKey, out bool cachedRevoked))
                {
                    return cachedRevoked;
                }

                // Check in blacklist
                var isRevoked = await _context.RevokedTokens
                    .AnyAsync(rt => rt.Token == token, cancellationToken);

                // Cache result for short time to reduce DB pressure
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };

                _cache.Set(cacheKey, isRevoked, cacheOptions);

                return isRevoked;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if token is revoked");
                // Fail-safe: If error checking blacklist, allow the request to proceed
                // The token validation will still happen
                return false;
            }
        }

        public async Task<int> RevokeAllUserTokensAsync(
            int userId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Note: This is a simplified implementation
                // In production, you'd need to track all active tokens for a user
                // For now, we'll just log this action
                _logger.LogWarning(
                    "Revoke all tokens requested for user {UserId}. Reason: {Reason}",
                    userId, reason);

                // In a complete implementation, you would:
                // 1. Query all active UserSessions for this user
                // 2. For each session, revoke the token
                // 3. Mark sessions as inactive

                var activeSessions = await _context.UserSessions
                    .Where(s => s.UserId == userId && s.IsActive == true)
                    .ToListAsync(cancellationToken);

                foreach (var session in activeSessions)
                {
                    session.IsActive = false;
                    session.LogoutTime = DateTime.UtcNow;

                    // Also remove from cache if we cached token
                    if (!string.IsNullOrWhiteSpace(session.SessionToken))
                    {
                        _cache.Remove(CachePrefix + session.SessionToken);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Revoked all tokens for user {UserId}. Sessions invalidated: {Count}",
                    userId, activeSessions.Count);

                return activeSessions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Delete expired tokens from blacklist
                var expiredTokens = await _context.RevokedTokens
                    .Where(rt => rt.ExpiresAt < now)
                    .ToListAsync(cancellationToken);

                if (expiredTokens.Any())
                {
                    // Remove their cache entries as well
                    foreach (var t in expiredTokens)
                    {
                        _cache.Remove(CachePrefix + t.Token);
                    }

                    _context.RevokedTokens.RemoveRange(expiredTokens);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Cleaned up {Count} expired tokens from blacklist",
                        expiredTokens.Count);

                    return expiredTokens.Count;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired tokens");
                throw;
            }
        }

        public DateTime? GetTokenExpiry(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(token))
                {
                    return null;
                }

                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse token expiry");
                return null;
            }
        }
    }
}
