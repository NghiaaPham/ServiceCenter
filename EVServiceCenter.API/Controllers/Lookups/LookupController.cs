using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.API.Controllers.Lookups  
{
    [ApiController]
    [Route("api/lookups")]
    public class LookupController : BaseController  
    {
        private readonly EVDbContext _context;

        public LookupController(EVDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all lookup data for frontend initialization
        /// Cached for 1 hour for performance
        /// </summary>
        [HttpGet]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAllLookups()
        {
            var lookups = new
            {
                AppointmentStatuses = await _context.AppointmentStatuses
                    .Where(s => s.IsActive == true)
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new
                    {
                        s.StatusId,
                        s.StatusName,
                        s.StatusColor,
                        s.Description,
                        s.DisplayOrder
                    })
                    .ToListAsync(),

                ServiceCategories = await _context.ServiceCategories
                    .Where(c => c.IsActive == true)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new
                    {
                        c.CategoryId,
                        c.CategoryName,
                        c.Description,
                        c.IconUrl
                    })
                    .ToListAsync()
            };

            return Ok(ApiResponse<object>.WithSuccess(
                lookups,
                "Lấy dữ liệu lookup thành công"
            ));
        }

        /// <summary>
        /// Get appointment statuses only
        /// </summary>
        [HttpGet("appointment-statuses")]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> GetAppointmentStatuses()
        {
            var statuses = await _context.AppointmentStatuses
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new
                {
                    s.StatusId,
                    s.StatusName,
                    s.StatusColor,
                    s.Description,
                    s.DisplayOrder
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.WithSuccess(
                statuses,
                $"Tìm thấy {statuses.Count} trạng thái"
            ));
        }
    }
}