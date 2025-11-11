using EVServiceCenter.Core.Domains.Testimonials.Interfaces;
using EVServiceCenter.Core.Domains.Testimonials.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Testimonials
{
    [ApiController]
    [Route("api/testimonials")]
    [AllowAnonymous]
    public class TestimonialsController : ControllerBase
    {
        private readonly Core.Domains.Testimonials.Interfaces.ITestimonialService _service;
        private readonly ILogger<TestimonialsController> _logger;

        public TestimonialsController(Core.Domains.Testimonials.Interfaces.ITestimonialService service, ILogger<TestimonialsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetByModel([FromQuery] int modelId, [FromQuery] int top = 5, CancellationToken ct = default)
        {
            if (modelId <= 0)
                return BadRequest(ApiResponse<object>.WithError("modelId không h?p l?", "INVALID_PARAMETER"));

            try
            {
                var result = await _service.GetTestimonialsByModelAsync(modelId, top, ct);
                return Ok(ApiResponse<List<TestimonialDto>>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting testimonials for model {ModelId}", modelId);
                return StatusCode(500, ApiResponse<object>.WithError("Có l?i khi l?y ?ánh giá", "INTERNAL_ERROR"));
            }
        }
    }
}
