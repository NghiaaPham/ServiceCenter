using EVServiceCenter.Core.Domains.Testimonials.DTOs.Responses;
using EVServiceCenter.Core.Domains.Testimonials.Interfaces;
using EVServiceCenter.Infrastructure.Domains.Testimonials.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace EVServiceCenter.Infrastructure.Domains.Testimonials.Services
{
    public class TestimonialService : ITestimonialService
    {
        private readonly TestimonialQueryRepository _repo;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TestimonialService> _logger;

        public TestimonialService(TestimonialQueryRepository repo, IMemoryCache cache, ILogger<TestimonialService> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<TestimonialDto>> GetTestimonialsByModelAsync(int modelId, int top = 5, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"testimonials_model_{modelId}_{top}";
            if (_cache.TryGetValue<List<TestimonialDto>>(cacheKey, out var cached))
                return cached;

            var result = await _repo.GetTestimonialsByModelAsync(modelId, top, cancellationToken);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }
    }
}