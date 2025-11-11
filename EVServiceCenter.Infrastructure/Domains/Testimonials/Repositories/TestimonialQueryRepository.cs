using EVServiceCenter.Core.Domains.Testimonials.DTOs.Responses;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EVServiceCenter.Infrastructure.Domains.Testimonials.Repositories
{
    public class TestimonialQueryRepository
    {
        private readonly EVDbContext _context;
        private readonly ILogger<TestimonialQueryRepository> _logger;

        public TestimonialQueryRepository(EVDbContext context, ILogger<TestimonialQueryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<TestimonialDto>> GetTestimonialsByModelAsync(int modelId, int top = 5, CancellationToken cancellationToken = default)
        {
            // Join ServiceRatings -> WorkOrder -> Vehicle -> Model
            var query = _context.ServiceRatings
                .AsNoTracking()
                .Where(r => r.WorkOrder != null && r.WorkOrder.Vehicle != null && r.WorkOrder.Vehicle.ModelId == modelId);

            var results = await query
                .OrderByDescending(r => r.OverallRating ?? 0)
                .ThenByDescending(r => r.RatingDate)
                .Take(top)
                .Select(r => new TestimonialDto
                {
                    RatingId = r.RatingId,
                    WorkOrderId = r.WorkOrderId,
                    CustomerName = r.Customer.FullName,
                    OverallRating = r.OverallRating,
                    PositiveFeedback = r.PositiveFeedback,
                    NegativeFeedback = r.NegativeFeedback,
                    Suggestions = r.Suggestions,
                    RatingDate = r.RatingDate,
                    ServiceCenterName = r.WorkOrder.ServiceCenter.CenterName,
                    VehicleModelName = r.WorkOrder.Vehicle.Model.ModelName
                })
                .ToListAsync(cancellationToken);

            return results;
        }
    }
}