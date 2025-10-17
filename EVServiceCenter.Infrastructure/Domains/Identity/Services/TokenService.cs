// EVServiceCenter.Infrastructure/Domains/Identity/Services/TokenService.cs
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.Interfaces;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
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
        private readonly EVDbContext _context; 

        public TokenService(IConfiguration config, EVDbContext context)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string GenerateToken(User user, int? customerId = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, ((UserRoles)user.RoleId).ToString()),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("FullName", user.FullName)
            };

            // ✅ FIX: Nếu là Customer role → Load Customer data và thêm vào claims
            if (user.RoleId == (int)UserRoles.Customer)
            {
                // Option 1: Customer ID được truyền vào (từ Login)
                if (customerId.HasValue)
                {
                    var customer = _context.Customers
                        .Include(c => c.Type)
                        .FirstOrDefault(c => c.CustomerId == customerId.Value);

                    if (customer != null)
                    {
                        claims.Add(new Claim("CustomerId", customer.CustomerId.ToString()));
                        claims.Add(new Claim("CustomerCode", customer.CustomerCode));
                        claims.Add(new Claim("CustomerType", customer.TypeId?.ToString() ?? "1"));
                        claims.Add(new Claim("LoyaltyPoints", customer.LoyaltyPoints?.ToString() ?? "0"));
                    }
                }
                // Option 2: Customer ID KHÔNG được truyền vào → Tìm Customer theo UserId
                else
                {
                    var customer = _context.Customers
                        .Include(c => c.Type)
                        .FirstOrDefault(c => c.UserId == user.UserId);

                    if (customer != null)
                    {
                        claims.Add(new Claim("CustomerId", customer.CustomerId.ToString()));
                        claims.Add(new Claim("CustomerCode", customer.CustomerCode));
                        claims.Add(new Claim("CustomerType", customer.TypeId?.ToString() ?? "1"));
                        claims.Add(new Claim("LoyaltyPoints", customer.LoyaltyPoints?.ToString() ?? "0"));
                    }
                }
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