using EVServiceCenter.Core.Domains.Testimonials.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.Testimonials.Interfaces
{
    public interface ITestimonialService
    {
        Task<List<TestimonialDto>> GetTestimonialsByModelAsync(int modelId, int top = 5, CancellationToken cancellationToken = default);
    }
}