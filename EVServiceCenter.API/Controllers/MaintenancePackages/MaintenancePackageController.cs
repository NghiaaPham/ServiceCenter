using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.MaintenancePackages
{
    [ApiController]
    [Route("api/maintenance-packages")]
    [Authorize(Policy = "AllInternal")]
    [ApiExplorerSettings(GroupName = "Staff - Packages")]
    public class MaintenancePackageController : BaseController
    {
        private readonly IMaintenancePackageService _service;
        private readonly IValidator<CreateMaintenancePackageRequestDto> _createValidator;
        private readonly IValidator<UpdateMaintenancePackageRequestDto> _updateValidator;
        private readonly IValidator<MaintenancePackageQueryDto> _queryValidator;
        private readonly ILogger<MaintenancePackageController> _logger;

        public MaintenancePackageController(
            IMaintenancePackageService service,
            IValidator<CreateMaintenancePackageRequestDto> createValidator,
            IValidator<UpdateMaintenancePackageRequestDto> updateValidator,
            IValidator<MaintenancePackageQueryDto> queryValidator,
            ILogger<MaintenancePackageController> logger)
        {
            _service = service;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _queryValidator = queryValidator;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách packages với pagination và filters
        /// Hỗ trợ search, filter theo status, giá, discount, popularity
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] MaintenancePackageQueryDto query,
            CancellationToken ct)
        {
            var validation = await _queryValidator.ValidateAsync(query, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<PagedResult<MaintenancePackageSummaryDto>>.WithValidationError(errors));
            }

            try
            {
                var result = await _service.GetAllPackagesAsync(query, ct);
                return Ok(ApiResponse<PagedResult<MaintenancePackageSummaryDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.TotalCount} gói dịch vụ"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all maintenance packages");
                return StatusCode(500, ApiResponse<PagedResult<MaintenancePackageSummaryDto>>.WithError(
                    "Có lỗi xảy ra khi lấy danh sách gói dịch vụ", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lấy chi tiết package theo ID
        /// Trả về full details bao gồm tất cả services trong package
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetPackageByIdAsync(id, ct);
                if (result == null)
                {
                    return NotFound(ApiResponse<MaintenancePackageResponseDto>.WithNotFound(
                        $"Không tìm thấy gói dịch vụ với ID: {id}"));
                }

                return Ok(ApiResponse<MaintenancePackageResponseDto>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance package {Id}", id);
                return StatusCode(500, ApiResponse<MaintenancePackageResponseDto>.WithError(
                    "Có lỗi xảy ra khi lấy thông tin gói dịch vụ", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lấy package theo PackageCode (unique identifier)
        /// VD: PKG-BASIC-2025
        /// </summary>
        [HttpGet("code/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetPackageByCodeAsync(code, ct);
                if (result == null)
                {
                    return NotFound(ApiResponse<MaintenancePackageResponseDto>.WithNotFound(
                        $"Không tìm thấy gói dịch vụ với mã: {code}"));
                }

                return Ok(ApiResponse<MaintenancePackageResponseDto>.WithSuccess(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance package by code {Code}", code);
                return StatusCode(500, ApiResponse<MaintenancePackageResponseDto>.WithError(
                    "Có lỗi xảy ra khi lấy thông tin gói dịch vụ", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Lấy danh sách packages phổ biến
        /// Chỉ trả về packages với IsPopular = true và Status = Active
        /// Public endpoint - không cần auth
        /// </summary>
        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopular(
            CancellationToken ct,
            [FromQuery] int topCount = 5)
        {
            try
            {
                if (topCount <= 0 || topCount > 20)
                {
                    return BadRequest(ApiResponse<List<MaintenancePackageSummaryDto>>.WithError(
                        "TopCount phải từ 1-20", "INVALID_PARAMETER"));
                }

                var result = await _service.GetPopularPackagesAsync(topCount, ct);
                return Ok(ApiResponse<List<MaintenancePackageSummaryDto>>.WithSuccess(
                    result,
                    $"Tìm thấy {result.Count} gói phổ biến"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular packages");
                return StatusCode(500, ApiResponse<List<MaintenancePackageSummaryDto>>.WithError(
                    "Có lỗi xảy ra khi lấy danh sách gói phổ biến", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Tạo package mới
        /// Chỉ Admin mới có quyền
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(
            [FromBody] CreateMaintenancePackageRequestDto request,
            CancellationToken ct)
        {
            var validation = await _createValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<MaintenancePackageResponseDto>.WithValidationError(errors));
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _service.CreatePackageAsync(request, currentUserId, ct);

                _logger.LogInformation("Maintenance package created by user {UserId}: {PackageCode}",
                    currentUserId, result.PackageCode);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.PackageId },
                    ApiResponse<MaintenancePackageResponseDto>.WithSuccess(
                        result,
                        "Tạo gói dịch vụ thành công",
                        201));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MaintenancePackageResponseDto>.WithError(
                    ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance package");
                return StatusCode(500, ApiResponse<MaintenancePackageResponseDto>.WithError(
                    "Có lỗi xảy ra khi tạo gói dịch vụ", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Cập nhật package
        /// Chỉ Admin mới có quyền
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateMaintenancePackageRequestDto request,
            CancellationToken ct)
        {
            if (id != request.PackageId)
            {
                return BadRequest(ApiResponse<MaintenancePackageResponseDto>.WithError(
                    "ID trong URL và body không khớp", "ID_MISMATCH"));
            }

            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                });
                return BadRequest(ApiResponse<MaintenancePackageResponseDto>.WithValidationError(errors));
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _service.UpdatePackageAsync(request, currentUserId, ct);

                _logger.LogInformation("Maintenance package updated by user {UserId}: {PackageId}",
                    currentUserId, id);

                return Ok(ApiResponse<MaintenancePackageResponseDto>.WithSuccess(
                    result,
                    "Cập nhật gói dịch vụ thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MaintenancePackageResponseDto>.WithError(
                    ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance package {Id}", id);
                return StatusCode(500, ApiResponse<MaintenancePackageResponseDto>.WithError(
                    "Có lỗi xảy ra khi cập nhật gói dịch vụ", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Xóa package (soft delete)
        /// Chỉ Admin mới có quyền
        /// Không thể xóa nếu đang có khách hàng sử dụng (active subscriptions)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _service.DeletePackageAsync(id, ct);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.WithNotFound(
                        $"Không tìm thấy gói dịch vụ với ID: {id}"));
                }

                _logger.LogInformation("Maintenance package deleted by user {UserId}: {PackageId}",
                    currentUserId, id);

                return Ok(ApiResponse<object>.WithSuccess(null, "Xóa gói dịch vụ thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.WithError(
                    ex.Message, "BUSINESS_RULE_VIOLATION"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting maintenance package {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError(
                    "Có lỗi xảy ra khi xóa gói dịch vụ", "INTERNAL_ERROR", 500));
            }
        }

        /// <summary>
        /// Kiểm tra xem package có thể xóa không
        /// Trả về { CanDelete: true/false, Reason: "..." }
        /// </summary>
        [HttpGet("{id:int}/can-delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CanDelete(int id, CancellationToken ct)
        {
            try
            {
                var canDelete = await _service.CanDeletePackageAsync(id, ct);
                var data = new
                {
                    CanDelete = canDelete,
                    Reason = canDelete
                        ? null
                        : "Gói dịch vụ đang có khách hàng sử dụng (active subscriptions) hoặc không tồn tại"
                };

                return Ok(ApiResponse<object>.WithSuccess(data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking can delete package {Id}", id);
                return StatusCode(500, ApiResponse<object>.WithError(
                    "Có lỗi xảy ra", "INTERNAL_ERROR", 500));
            }
        }
    }
}
