using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace EVServiceCenter.Infrastructure.Domains.MaintenancePackages.Services
{
    /// <summary>
    /// Service Implementation cho Maintenance Package
    /// Chứa business logic, validation, orchestrate repository calls
    /// Sử dụng cả Query và Command repositories (CQRS pattern)
    /// </summary>
    public class MaintenancePackageService : IMaintenancePackageService
    {
        private readonly IMaintenancePackageQueryRepository _queryRepository;
        private readonly IMaintenancePackageCommandRepository _commandRepository;
        private readonly ILogger<MaintenancePackageService> _logger;
        private readonly IMemoryCache _cache;

        public MaintenancePackageService(
            IMaintenancePackageQueryRepository queryRepository,
            IMaintenancePackageCommandRepository commandRepository,
            ILogger<MaintenancePackageService> logger,
            IMemoryCache cache)
        {
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
            _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        #region Query Methods - Delegate to QueryRepository

        public async Task<PagedResult<MaintenancePackageSummaryDto>> GetAllPackagesAsync(
            MaintenancePackageQueryDto query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Getting all packages with query: {@Query}", query);
                return await _queryRepository.GetAllPackagesAsync(query, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error getting all packages");
                throw;
            }
        }

        public async Task<MaintenancePackageResponseDto?> GetPackageByIdAsync(
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Getting package by ID: {PackageId}", packageId);
                return await _queryRepository.GetPackageByIdAsync(packageId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error getting package by ID: {PackageId}", packageId);
                throw;
            }
        }

        public async Task<MaintenancePackageResponseDto?> GetPackageByCodeAsync(
            string packageCode,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Getting package by code: {PackageCode}", packageCode);
                return await _queryRepository.GetPackageByCodeAsync(packageCode, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error getting package by code: {PackageCode}", packageCode);
                throw;
            }
        }

        public async Task<List<MaintenancePackageSummaryDto>> GetPopularPackagesAsync(
            int topCount = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Getting top {TopCount} popular packages", topCount);
                return await _queryRepository.GetPopularPackagesAsync(topCount, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error getting popular packages");
                throw;
            }
        }

        /// <summary>
        /// Lấy các gói khuyến nghị cho modelId - delegate to query repository
        /// Caching applied to reduce DB pressure for repeated FE calls
        /// </summary>
        public async Task<List<MaintenancePackageSummaryDto>> GetRecommendedPackagesAsync(
            int modelId,
            int topCount = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Getting recommended packages for model {ModelId} top {TopCount}", modelId, topCount);

                var cacheKey = $"recommended_packages_{modelId}_{topCount}";

                if (_cache.TryGetValue<List<MaintenancePackageSummaryDto>>(cacheKey, out var cached))
                {
                    return cached;
                }

                var result = await _queryRepository.GetRecommendedPackagesAsync(modelId, topCount, cancellationToken);

                // Cache for short time (5 minutes)
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, result, cacheOptions);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error getting recommended packages for model {ModelId}", modelId);
                throw;
            }
        }

        #endregion

        #region Command Methods - Add Business Logic Validation

        public async Task<MaintenancePackageResponseDto> CreatePackageAsync(
            CreateMaintenancePackageRequestDto request,
            int createdByUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Creating package: {PackageCode} by user {UserId}",
                    request.PackageCode, createdByUserId);

                // ========== BUSINESS VALIDATION ==========

                var (isValid, errorMessage) = await ValidateCreatePackageRequestAsync(request, cancellationToken);
                if (!isValid)
                {
                    throw new InvalidOperationException($"Validation failed: {errorMessage}");
                }

                // ========== DELEGATE TO COMMAND REPOSITORY ==========

                var result = await _commandRepository.CreatePackageAsync(
                    request, createdByUserId, cancellationToken);

                _logger.LogInformation("Service: Successfully created package: {PackageCode}", result.PackageCode);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error creating package: {PackageCode}", request.PackageCode);
                throw;
            }
        }

        public async Task<MaintenancePackageResponseDto> UpdatePackageAsync(
            UpdateMaintenancePackageRequestDto request,
            int updatedByUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Updating package: {PackageId} by user {UserId}",
                    request.PackageId, updatedByUserId);

                // ========== BUSINESS VALIDATION ==========

                var (isValid, errorMessage) = await ValidateUpdatePackageRequestAsync(request, cancellationToken);
                if (!isValid)
                {
                    throw new InvalidOperationException($"Validation failed: {errorMessage}");
                }

                // ========== DELEGATE TO COMMAND REPOSITORY ==========

                var result = await _commandRepository.UpdatePackageAsync(
                    request, updatedByUserId, cancellationToken);

                _logger.LogInformation("Service: Successfully updated package: {PackageCode}", result.PackageCode);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error updating package: {PackageId}", request.PackageId);
                throw;
            }
        }

        public async Task<bool> DeletePackageAsync(
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Attempting to delete package: {PackageId}", packageId);

                // ========== BUSINESS VALIDATION ==========

                // Check if package can be safely deleted
                var canDelete = await CanDeletePackageAsync(packageId, cancellationToken);
                if (!canDelete)
                {
                    throw new InvalidOperationException(
                        "Không thể xóa gói này vì đang có khách hàng sử dụng (active subscriptions)");
                }

                // ========== DELEGATE TO COMMAND REPOSITORY ==========

                var result = await _commandRepository.SoftDeletePackageAsync(packageId, cancellationToken);

                if (result)
                {
                    _logger.LogInformation("Service: Successfully deleted package: {PackageId}", packageId);
                }
                else
                {
                    _logger.LogWarning("Service: Package not found for deletion: {PackageId}", packageId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error deleting package: {PackageId}", packageId);
                throw;
            }
        }

        #endregion

        #region Business Validation Methods

        public async Task<bool> CanDeletePackageAsync(
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check package exists
                var exists = await _queryRepository.PackageExistsAsync(packageId, cancellationToken);
                if (!exists)
                {
                    _logger.LogWarning("Service: Package does not exist: {PackageId}", packageId);
                    return false;
                }

                // Check for active subscriptions
                var hasActiveSubscriptions = await _queryRepository.HasActiveSubscriptionsAsync(
                    packageId, cancellationToken);

                if (hasActiveSubscriptions)
                {
                    _logger.LogWarning("Service: Cannot delete package {PackageId} - has active subscriptions",
                        packageId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error checking if package can be deleted: {PackageId}", packageId);
                throw;
            }
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateCreatePackageRequestAsync(
            CreateMaintenancePackageRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // ========== VALIDATE PACKAGE CODE ==========

                if (string.IsNullOrWhiteSpace(request.PackageCode))
                {
                    return (false, "Mã gói không được để trống");
                }

                // Check uniqueness (already checked in repository, but good to validate early)
                var codeExists = await _queryRepository.IsPackageCodeExistsAsync(
                    request.PackageCode, null, cancellationToken);

                if (codeExists)
                {
                    return (false, $"Mã gói '{request.PackageCode}' đã tồn tại trong hệ thống");
                }

                // ========== VALIDATE SERVICES ==========

                if (request.IncludedServices == null || request.IncludedServices.Count == 0)
                {
                    return (false, "Gói phải chứa ít nhất 1 dịch vụ");
                }

                // Check for duplicate services in request
                var duplicateServices = request.IncludedServices
                    .GroupBy(s => s.ServiceId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateServices.Any())
                {
                    return (false, $"Có dịch vụ trùng lặp trong gói: ServiceId {string.Join(", ", duplicateServices)}");
                }

                // Validate quantity > 0
                var invalidQuantity = request.IncludedServices
                    .Where(s => s.QuantityInPackage <= 0)
                    .ToList();

                if (invalidQuantity.Any())
                {
                    return (false, "Số lượng dịch vụ trong gói phải > 0");
                }

                // ========== VALIDATE PRICING ==========

                if (request.TotalPriceAfterDiscount < 0)
                {
                    return (false, "Giá gói không thể âm");
                }

                // Validate discount percent (if provided)
                if (request.DiscountPercent.HasValue)
                {
                    if (request.DiscountPercent.Value < 0 || request.DiscountPercent.Value > 100)
                    {
                        return (false, "Phần trăm giảm giá phải từ 0-100%");
                    }
                }

                // ========== VALIDATE VALIDITY PERIOD/MILEAGE ==========

                if (!request.ValidityPeriodInDays.HasValue && !request.ValidityMileage.HasValue)
                {
                    return (false, "Gói phải có ít nhất 1 điều kiện hết hạn (thời gian hoặc số km)");
                }

                if (request.ValidityPeriodInDays.HasValue && request.ValidityPeriodInDays.Value <= 0)
                {
                    return (false, "Thời hạn gói phải > 0 ngày");
                }

                if (request.ValidityMileage.HasValue && request.ValidityMileage.Value <= 0)
                {
                    return (false, "Số km hiệu lực phải > 0");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error validating create package request");
                throw;
            }
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateUpdatePackageRequestAsync(
            UpdateMaintenancePackageRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // ========== CHECK PACKAGE EXISTS ==========

                var exists = await _queryRepository.PackageExistsAsync(request.PackageId, cancellationToken);
                if (!exists)
                {
                    return (false, $"Không tìm thấy gói với ID: {request.PackageId}");
                }

                // ========== VALIDATE PACKAGE CODE ==========

                if (string.IsNullOrWhiteSpace(request.PackageCode))
                {
                    return (false, "Mã gói không được để trống");
                }

                // Check uniqueness (exclude current package)
                var codeExists = await _queryRepository.IsPackageCodeExistsAsync(
                    request.PackageCode, request.PackageId, cancellationToken);

                if (codeExists)
                {
                    return (false, $"Mã gói '{request.PackageCode}' đã được sử dụng bởi gói khác");
                }

                // ========== VALIDATE SERVICES ==========

                if (request.IncludedServices == null || request.IncludedServices.Count == 0)
                {
                    return (false, "Gói phải chứa ít nhất 1 dịch vụ");
                }

                // Check for duplicate services
                var duplicateServices = request.IncludedServices
                    .GroupBy(s => s.ServiceId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateServices.Any())
                {
                    return (false, $"Có dịch vụ trùng lặp trong gói: ServiceId {string.Join(", ", duplicateServices)}");
                }

                // Validate quantity > 0
                var invalidQuantity = request.IncludedServices
                    .Where(s => s.QuantityInPackage <= 0)
                    .ToList();

                if (invalidQuantity.Any())
                {
                    return (false, "Số lượng dịch vụ trong gói phải > 0");
                }

                // ========== VALIDATE PRICING ==========

                if (request.TotalPriceAfterDiscount < 0)
                {
                    return (false, "Giá gói không thể âm");
                }

                if (request.DiscountPercent.HasValue)
                {
                    if (request.DiscountPercent.Value < 0 || request.DiscountPercent.Value > 100)
                    {
                        return (false, "Phần trăm giảm giá phải từ 0-100%");
                    }
                }

                // ========== VALIDATE VALIDITY PERIOD/MILEAGE ==========

                if (!request.ValidityPeriodInDays.HasValue && !request.ValidityMileage.HasValue)
                {
                    return (false, "Gói phải có ít nhất 1 điều kiện hết hạn (thời gian hoặc số km)");
                }

                if (request.ValidityPeriodInDays.HasValue && request.ValidityPeriodInDays.Value <= 0)
                {
                    return (false, "Thời hạn gói phải > 0 ngày");
                }

                if (request.ValidityMileage.HasValue && request.ValidityMileage.Value <= 0)
                {
                    return (false, "Số km hiệu lực phải > 0");
                }

                // ========== OPTIONAL: CHECK ACTIVE SUBSCRIPTIONS ==========
                // Uncomment nếu muốn NGĂN update package đang có subscription active
                // Business rule: Tùy yêu cầu, có thể cho phép update hoặc không

                /*
                var hasActiveSubscriptions = await _queryRepository.HasActiveSubscriptionsAsync(
                    request.PackageId, cancellationToken);

                if (hasActiveSubscriptions)
                {
                    return (false, "Không thể cập nhật gói đang có khách hàng sử dụng (active subscriptions)");
                }
                */

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error validating update package request");
                throw;
            }
        }

        #endregion
    }
}
