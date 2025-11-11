using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Configuration;
using EVServiceCenter.Infrastructure.Domains.Identity.Services;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.Identity.DTOs;
using EVServiceCenter.Core.Helpers;

namespace EVServiceCenter.Benchmarks
{
    [MemoryDiagnoser]
    public class AuthBenchmarks
    {
        private TokenService _tokenService;
        private EVServiceCenter.Core.Domains.Identity.Entities.User _user;
        private TokenCustomerInfo _customerInfo;
        private string _plainPassword = "P@ssw0rd123";
        private string _salt;
        private string _hash;

        [GlobalSetup]
        public void Setup()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                ["JwtSettings:SecretKey"] = "verysecretkey1234567890123456",
                ["JwtSettings:Issuer"] = "issuer",
                ["JwtSettings:Audience"] = "aud",
                ["JwtSettings:ExpirationMinutes"] = "60"
            };

            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            _tokenService = new TokenService(config);

            _user = new EVServiceCenter.Core.Domains.Identity.Entities.User { UserId = 1, Username = "test", RoleId = (int)EVServiceCenter.Core.Enums.UserRoles.Customer, Email = "a@b.c", FullName = "Test" };
            _customerInfo = new TokenCustomerInfo { CustomerId = 10, CustomerCode = "C123", CustomerTypeId = 1, LoyaltyPoints = 0 };

            _salt = SecurityHelper.GenerateSalt();
            _hash = SecurityHelper.HashPassword(_plainPassword, _salt);
        }

        [Benchmark]
        public string GenerateJwt() => _tokenService.GenerateToken(_user, _customerInfo);

        [Benchmark]
        public bool VerifyPassword() => SecurityHelper.VerifyPassword(_plainPassword, _hash);

        [Benchmark]
        public string HashPassword() => SecurityHelper.HashPassword(_plainPassword, SecurityHelper.GenerateSalt());
    }
}