// EVServiceCenter.Infrastructure/Domains/Identity/Services/TokenService.cs
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Domains.Identity.DTOs;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EVServiceCenter.Infrastructure.Domains.Identity.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public string GenerateToken(User user, TokenCustomerInfo? customerInfo = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("UserId", user.UserId.ToString()), // Add explicit UserId claim
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, ((UserRoles)user.RoleId).ToString()),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("FullName", user.FullName ?? "")
            };

            if (user.RoleId == (int)UserRoles.Customer && customerInfo != null)
            {
                claims.Add(new Claim("CustomerId", customerInfo.CustomerId.ToString()));
                if (!string.IsNullOrEmpty(customerInfo.CustomerCode))
                    claims.Add(new Claim("CustomerCode", customerInfo.CustomerCode));
                if (customerInfo.CustomerTypeId.HasValue)
                    claims.Add(new Claim("CustomerType", customerInfo.CustomerTypeId.Value.ToString()));
                claims.Add(new Claim("LoyaltyPoints", customerInfo.LoyaltyPoints.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:ExpirationMinutes"]!)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}