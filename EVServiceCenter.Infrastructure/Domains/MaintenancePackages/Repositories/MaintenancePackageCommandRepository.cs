using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.MaintenancePackages.Repositories
{
    /// <summary>
    /// Command Repository Implementation cho Maintenance Package
    /// Chứa tất cả WRITE operations - Create, Update, Delete
    /// Thay đổi state của database
    /// </summary>
    public class MaintenancePackageCommandRepository : IMaintenancePackageCommandRepository
    {
        private readonly EVDbContext _context;
        private readonly IMaintenancePackageQueryRepository _queryRepository;
        private readonly ILogger<MaintenancePackageCommandRepository> _logger;

        public MaintenancePackageCommandRepository(
            EVDbContext context,
            IMaintenancePackageQueryRepository queryRepository,
            ILogger<MaintenancePackageCommandRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region CreatePackageAsync

        public async Task<MaintenancePackageResponseDto> CreatePackageAsync(
            CreateMaintenancePackageRequestDto request,
            int createdByUserId,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Creating new package: {PackageCode}", request.PackageCode);

                // ========== VALIDATE ==========

                // Check PackageCode unique
                if (await _queryRepository.IsPackageCodeExistsAsync(request.PackageCode, null, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã gói '{request.PackageCode}' đã tồn tại trong hệ thống");
                }

                // Validate services exist
                var serviceIds = request.IncludedServices.Select(s => s.ServiceId).ToList();
                var existingServices = await _context.MaintenanceServices
                    .Where(s => serviceIds.Contains(s.ServiceId))
                    .ToListAsync(cancellationToken);

                if (existingServices.Count != serviceIds.Count)
                {
                    throw new InvalidOperationException("Một số dịch vụ trong gói không tồn tại");
                }

                // ========== CREATE PACKAGE ENTITY ==========

                var package = new MaintenancePackage
                {
                    PackageCode = request.PackageCode.Trim(),
                    PackageName = request.PackageName.Trim(),
                    Description = string.IsNullOrWhiteSpace(request.Description)
                        ? null
                        : request.Description.Trim(),
                    ValidityPeriod = request.ValidityPeriodInDays,
                    ValidityMileage = request.ValidityMileage,
                    TotalPrice = request.TotalPriceAfterDiscount,
                    DiscountPercent = request.DiscountPercent,
                    ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl)
                        ? null
                        : request.ImageUrl.Trim(),
                    IsPopular = request.IsPopularPackage,
                    IsActive = request.Status == PackageStatusEnum.Active,
                    CreatedDate = DateTime.UtcNow
                };

                _context.MaintenancePackages.Add(package);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Package created with ID: {PackageId}", package.PackageId);

                // ========== CREATE PACKAGE-SERVICE RELATIONS ==========

                var packageServices = request.IncludedServices.Select(s => new PackageService
                {
                    PackageId = package.PackageId,
                    ServiceId = s.ServiceId,
                    Quantity = s.QuantityInPackage,
                    IncludedInPackage = s.IsIncludedInPackagePrice,
                    AdditionalCost = s.AdditionalCostPerExtraQuantity
                }).ToList();

                _context.PackageServices.AddRange(packageServices);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created {Count} package-service relations for PackageId: {PackageId}",
                    packageServices.Count, package.PackageId);

                await transaction.CommitAsync(cancellationToken);

                // ========== RETURN FULL DETAILS ==========

                var result = await _queryRepository.GetPackageByIdAsync(package.PackageId, cancellationToken);
                if (result == null)
                {
                    throw new InvalidOperationException("Failed to retrieve created package");
                }

                _logger.LogInformation("Successfully created package: {PackageCode} - {PackageName}",
                    result.PackageCode, result.PackageName);

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error creating package: {PackageCode}", request.PackageCode);
                throw;
            }
        }

        #endregion

        #region UpdatePackageAsync

        public async Task<MaintenancePackageResponseDto> UpdatePackageAsync(
            UpdateMaintenancePackageRequestDto request,
            int updatedByUserId,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Updating package: {PackageId}", request.PackageId);

                // ========== LOAD EXISTING PACKAGE ==========

                var package = await _context.MaintenancePackages
                    .Include(p => p.PackageServices)
                    .FirstOrDefaultAsync(p => p.PackageId == request.PackageId, cancellationToken);

                if (package == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy gói với ID: {request.PackageId}");
                }

                // ========== VALIDATE ==========

                // Check PackageCode unique (exclude current package)
                if (await _queryRepository.IsPackageCodeExistsAsync(
                    request.PackageCode, request.PackageId, cancellationToken))
                {
                    throw new InvalidOperationException($"Mã gói '{request.PackageCode}' đã được sử dụng bởi gói khác");
                }

                // Validate services exist
                var serviceIds = request.IncludedServices.Select(s => s.ServiceId).ToList();
                var existingServices = await _context.MaintenanceServices
                    .Where(s => serviceIds.Contains(s.ServiceId))
                    .ToListAsync(cancellationToken);

                if (existingServices.Count != serviceIds.Count)
                {
                    throw new InvalidOperationException("Một số dịch vụ trong gói không tồn tại");
                }

                // ========== UPDATE PACKAGE PROPERTIES ==========

                package.PackageCode = request.PackageCode.Trim();
                package.PackageName = request.PackageName.Trim();
                package.Description = string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim();
                package.ValidityPeriod = request.ValidityPeriodInDays;
                package.ValidityMileage = request.ValidityMileage;
                package.TotalPrice = request.TotalPriceAfterDiscount;
                package.DiscountPercent = request.DiscountPercent;
                package.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl)
                    ? null
                    : request.ImageUrl.Trim();
                package.IsPopular = request.IsPopularPackage;
                package.IsActive = request.Status == PackageStatusEnum.Active;

                await _context.SaveChangesAsync(cancellationToken);

                // ========== UPDATE PACKAGE-SERVICE RELATIONS ==========

                // Delete old relations
                _context.PackageServices.RemoveRange(package.PackageServices);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Removed {Count} old package-service relations", package.PackageServices.Count);

                // Create new relations
                var newPackageServices = request.IncludedServices.Select(s => new PackageService
                {
                    PackageId = package.PackageId,
                    ServiceId = s.ServiceId,
                    Quantity = s.QuantityInPackage,
                    IncludedInPackage = s.IsIncludedInPackagePrice,
                    AdditionalCost = s.AdditionalCostPerExtraQuantity
                }).ToList();

                _context.PackageServices.AddRange(newPackageServices);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created {Count} new package-service relations", newPackageServices.Count);

                await transaction.CommitAsync(cancellationToken);

                // ========== RETURN UPDATED DETAILS ==========

                var result = await _queryRepository.GetPackageByIdAsync(package.PackageId, cancellationToken);
                if (result == null)
                {
                    throw new InvalidOperationException("Failed to retrieve updated package");
                }

                _logger.LogInformation("Successfully updated package: {PackageCode} - {PackageName}",
                    result.PackageCode, result.PackageName);

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error updating package: {PackageId}", request.PackageId);
                throw;
            }
        }

        #endregion

        #region SoftDeletePackageAsync

        public async Task<bool> SoftDeletePackageAsync(
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Soft deleting package: {PackageId}", packageId);

                var package = await _context.MaintenancePackages
                    .FirstOrDefaultAsync(p => p.PackageId == packageId, cancellationToken);

                if (package == null)
                {
                    _logger.LogWarning("Package not found for soft delete: {PackageId}", packageId);
                    return false;
                }

                // Set IsActive = false (soft delete)
                package.IsActive = false;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully soft deleted package: {PackageId} - {PackageCode}",
                    packageId, package.PackageCode);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting package: {PackageId}", packageId);
                throw;
            }
        }

        #endregion

        #region UpdatePackageStatusAsync

        public async Task<bool> UpdatePackageStatusAsync(
            int packageId,
            PackageStatusEnum newStatus,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating package status: {PackageId} to {NewStatus}", packageId, newStatus);

                var package = await _context.MaintenancePackages
                    .FirstOrDefaultAsync(p => p.PackageId == packageId, cancellationToken);

                if (package == null)
                {
                    _logger.LogWarning("Package not found: {PackageId}", packageId);
                    return false;
                }

                package.IsActive = newStatus == PackageStatusEnum.Active;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully updated package status: {PackageId} to {NewStatus}",
                    packageId, newStatus);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating package status: {PackageId}", packageId);
                throw;
            }
        }

        #endregion

        #region SetPackagePopularityAsync

        public async Task<bool> SetPackagePopularityAsync(
            int packageId,
            bool isPopular,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Setting package popularity: {PackageId} to {IsPopular}", packageId, isPopular);

                var package = await _context.MaintenancePackages
                    .FirstOrDefaultAsync(p => p.PackageId == packageId, cancellationToken);

                if (package == null)
                {
                    _logger.LogWarning("Package not found: {PackageId}", packageId);
                    return false;
                }

                package.IsPopular = isPopular;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully set package popularity: {PackageId} to {IsPopular}",
                    packageId, isPopular);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting package popularity: {PackageId}", packageId);
                throw;
            }
        }

        #endregion
    }
}
