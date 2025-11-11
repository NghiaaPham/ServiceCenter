using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.MaintenancePackages.Repositories
{
    /// <summary>
    /// Query Repository Implementation cho Maintenance Package
    /// Chứa tất cả READ operations - Không thay đổi data
    /// </summary>
    public class MaintenancePackageQueryRepository : IMaintenancePackageQueryRepository
    {
        private readonly EVDbContext _context;
        private readonly ILogger<MaintenancePackageQueryRepository> _logger;

        public MaintenancePackageQueryRepository(
            EVDbContext context,
            ILogger<MaintenancePackageQueryRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region GetAllPackagesAsync - Với Pagination & Filter

        public async Task<PagedResult<MaintenancePackageSummaryDto>> GetAllPackagesAsync(
            MaintenancePackageQueryDto query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var baseQuery = _context.MaintenancePackages.AsQueryable();

                // ========== APPLY FILTERS ==========

                // Filter by search term (PackageName or PackageCode)
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchTerm = query.SearchTerm.Trim().ToLower();
                    baseQuery = baseQuery.Where(p =>
                        p.PackageName.ToLower().Contains(searchTerm) ||
                        p.PackageCode.ToLower().Contains(searchTerm));
                }

                // Filter by Status
                if (query.Status.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.IsActive == (query.Status.Value == PackageStatusEnum.Active));
                }

                // Filter by IsPopularOnly
                if (query.IsPopularOnly.HasValue && query.IsPopularOnly.Value)
                {
                    baseQuery = baseQuery.Where(p => p.IsPopular == true);
                }

                // Filter by Price range
                if (query.MinPrice.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.TotalPrice >= query.MinPrice.Value);
                }

                if (query.MaxPrice.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.TotalPrice <= query.MaxPrice.Value);
                }

                // Filter by Discount %
                if (query.MinDiscountPercent.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.DiscountPercent.HasValue &&
                        p.DiscountPercent >= query.MinDiscountPercent.Value);
                }

                // ========== APPLY SORTING ==========
                IQueryable<MaintenancePackage> sortedQuery = query.SortDescending
                    ? query.SortBy switch
                    {
                        "Price" => baseQuery.OrderByDescending(p => p.TotalPrice),
                        "Name" => baseQuery.OrderByDescending(p => p.PackageName),
                        "Discount" => baseQuery.OrderByDescending(p => p.DiscountPercent),
                        "Popular" => baseQuery.OrderByDescending(p => p.IsPopular),
                        "CreatedDate" => baseQuery.OrderByDescending(p => p.CreatedDate),
                        _ => baseQuery.OrderByDescending(p => p.CreatedDate)
                    }
                    : query.SortBy switch
                    {
                        "Price" => baseQuery.OrderBy(p => p.TotalPrice),
                        "Name" => baseQuery.OrderBy(p => p.PackageName),
                        "Discount" => baseQuery.OrderBy(p => p.DiscountPercent),
                        "Popular" => baseQuery.OrderBy(p => p.IsPopular),
                        "CreatedDate" => baseQuery.OrderBy(p => p.CreatedDate),
                        _ => baseQuery.OrderBy(p => p.CreatedDate)
                    };

                // Get total count
                var totalCount = await sortedQuery.CountAsync(cancellationToken);

                // Get paged data với projections
                var items = await sortedQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Include(p => p.PackageServices)
                        .ThenInclude(ps => ps.Service)
                            .ThenInclude(s => s.Category)
                    .AsNoTracking()
                    .Select(p => MapToSummaryDto(p))
                    .ToListAsync(cancellationToken);

                return PagedResultFactory.Create(items, totalCount, query.Page, query.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all packages with query: {@Query}", query);
                throw;
            }
        }

        #endregion

        #region GetPackageByIdAsync - Chi tiết đầy đủ

        public async Task<MaintenancePackageResponseDto?> GetPackageByIdAsync(
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var package = await _context.MaintenancePackages
                    .Include(p => p.PackageServices)
                        .ThenInclude(ps => ps.Service)
                            .ThenInclude(s => s.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PackageId == packageId, cancellationToken);

                if (package == null)
                    return null;

                return await MapToResponseDtoAsync(package, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting package by ID: {PackageId}", packageId);
                throw;
            }
        }

        #endregion

        #region GetPackageByCodeAsync

        public async Task<MaintenancePackageResponseDto?> GetPackageByCodeAsync(
            string packageCode,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var package = await _context.MaintenancePackages
                    .Include(p => p.PackageServices)
                        .ThenInclude(ps => ps.Service)
                            .ThenInclude(s => s.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PackageCode == packageCode, cancellationToken);

                if (package == null)
                    return null;

                return await MapToResponseDtoAsync(package, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting package by code: {PackageCode}", packageCode);
                throw;
            }
        }

        #endregion

        #region GetPopularPackagesAsync

        public async Task<List<MaintenancePackageSummaryDto>> GetPopularPackagesAsync(
            int topCount = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var packages = await _context.MaintenancePackages
                    .Where(p => p.IsPopular == true && p.IsActive == true)
                    .OrderByDescending(p => p.CreatedDate) // Có thể sort theo số subscription sau
                    .Take(topCount)
                    .Include(p => p.PackageServices)
                        .ThenInclude(ps => ps.Service)
                            .ThenInclude(s => s.Category)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return packages.Select(p => MapToSummaryDto(p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular packages");
                throw;
            }
        }

        #endregion

        #region GetServicesInPackageAsync - Entities

        public async Task<List<MaintenanceService>> GetServicesInPackageAsync(
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.PackageServices
                    .Where(ps => ps.PackageId == packageId)
                    .Include(ps => ps.Service)
                    .AsNoTracking()
                    .Select(ps => ps.Service)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services in package: {PackageId}", packageId);
                throw;
            }
        }

        #endregion

        #region CalculateOriginalPriceBeforeDiscountAsync

        public async Task<decimal> CalculateOriginalPriceBeforeDiscountAsync(
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var total = await _context.PackageServices
                    .Where(ps => ps.PackageId == packageId)
                    .Include(ps => ps.Service)
                    .SumAsync(ps => ps.Service.BasePrice * (ps.Quantity ?? 1), cancellationToken);

                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating original price for package: {PackageId}", packageId);
                throw;
            }
        }

        #endregion

        #region Validation Methods

        public async Task<bool> IsPackageCodeExistsAsync(
            string packageCode,
            int? excludePackageId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _context.MaintenancePackages
                    .Where(p => p.PackageCode == packageCode);

                if (excludePackageId.HasValue)
                {
                    query = query.Where(p => p.PackageId != excludePackageId.Value);
                }

                return await query.AnyAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking package code exists: {PackageCode}", packageCode);
                throw;
            }
        }

        public async Task<bool> HasActiveSubscriptionsAsync(
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.CustomerPackageSubscriptions
                    .AnyAsync(s =>
                        s.PackageId == packageId &&
                        s.Status == "Active", // Sau này sẽ dùng enum
                        cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking active subscriptions for package: {PackageId}", packageId);
                throw;
            }
        }

        public async Task<bool> PackageExistsAsync(
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.MaintenancePackages
                    .AsNoTracking()
                    .AnyAsync(p => p.PackageId == packageId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking package exists: {PackageId}", packageId);
                throw;
            }
        }

        #endregion

        #region Private Mapping Methods

        /// <summary>
        /// Map entity to SummaryDto (for listing)
        /// </summary>
        private static MaintenancePackageSummaryDto MapToSummaryDto(MaintenancePackage package)
        {
            var services = package.PackageServices?.ToList() ?? new List<PackageService>();

            // Calculate metrics
            var totalServicesCount = services.Count;
            var totalQuantity = services.Sum(ps => ps.Quantity ?? 1);
            var totalTime = services.Sum(ps => ps.Service?.StandardTime ?? 0);
            var originalPrice = services.Sum(ps => (ps.Service?.BasePrice ?? 0) * (ps.Quantity ?? 1));

            // Service names preview (top 3)
            var serviceNames = services
                .Take(3)
                .Select(ps => ps.Service?.ServiceName ?? "")
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
            var preview = string.Join(", ", serviceNames);
            if (services.Count > 3)
                preview += "...";

            return new MaintenancePackageSummaryDto
            {
                PackageId = package.PackageId,
                PackageCode = package.PackageCode,
                PackageName = package.PackageName,
                ShortDescription = package.Description?.Length > 200
                    ? package.Description.Substring(0, 197) + "..."
                    : package.Description,
                TotalPriceAfterDiscount = package.TotalPrice ?? 0,
                OriginalPriceBeforeDiscount = originalPrice,
                SavingsAmount = originalPrice - (package.TotalPrice ?? 0),
                DiscountPercent = package.DiscountPercent,
                ImageUrl = package.ImageUrl,
                IsPopularPackage = package.IsPopular ?? false,
                Status = package.IsActive == true ? PackageStatusEnum.Active : PackageStatusEnum.Inactive,
                StatusDisplayName = package.IsActive == true ? "Đang hoạt động" : "Tạm ngưng",
                ValidityPeriodInDays = package.ValidityPeriod,
                ValidityMileage = package.ValidityMileage,
                TotalServicesCount = totalServicesCount,
                TotalServiceQuantity = totalQuantity,
                TotalEstimatedTimeInMinutes = totalTime,
                ServiceNamesPreview = preview,
                CreatedDate = package.CreatedDate ?? DateTime.UtcNow
            };
        }

        /// <summary>
        /// Map entity to ResponseDto (full details)
        /// </summary>
        private async Task<MaintenancePackageResponseDto> MapToResponseDtoAsync(
            MaintenancePackage package,
            CancellationToken cancellationToken)
        {
            var services = package.PackageServices?.ToList() ?? new List<PackageService>();

            // Calculate original price
            var originalPrice = await CalculateOriginalPriceBeforeDiscountAsync(
                package.PackageId, cancellationToken);

            var serviceDetails = services.Select(ps => new PackageServiceDetailResponseDto
            {
                PackageServiceId = ps.PackageServiceId,
                ServiceId = ps.ServiceId,
                ServiceName = ps.Service?.ServiceName ?? "",
                ServiceDescription = ps.Service?.Description,
                CategoryName = ps.Service?.Category?.CategoryName ?? "",
                ServiceBasePrice = ps.Service?.BasePrice ?? 0,
                StandardTimeInMinutes = ps.Service?.StandardTime ?? 0,
                QuantityInPackage = ps.Quantity ?? 1,
                IsIncludedInPackagePrice = ps.IncludedInPackage ?? true,
                AdditionalCostPerExtraQuantity = ps.AdditionalCost
            }).ToList();

            var totalTime = serviceDetails.Sum(s => s.StandardTimeInMinutes * s.QuantityInPackage);
            var totalQuantity = serviceDetails.Sum(s => s.QuantityInPackage);

            return new MaintenancePackageResponseDto
            {
                PackageId = package.PackageId,
                PackageCode = package.PackageCode,
                PackageName = package.PackageName,
                Description = package.Description,
                ValidityPeriodInDays = package.ValidityPeriod,
                ValidityMileage = package.ValidityMileage,
                TotalPriceAfterDiscount = package.TotalPrice ?? 0,
                OriginalPriceBeforeDiscount = originalPrice,
                SavingsAmount = originalPrice - (package.TotalPrice ?? 0),
                DiscountPercent = package.DiscountPercent,
                ImageUrl = package.ImageUrl,
                IsPopularPackage = package.IsPopular ?? false,
                Status = package.IsActive == true ? PackageStatusEnum.Active : PackageStatusEnum.Inactive,
                StatusDisplayName = package.IsActive == true ? "Đang hoạt động" : "Tạm ngưng",
                CreatedDate = package.CreatedDate ?? DateTime.UtcNow,
                IncludedServices = serviceDetails,
                TotalEstimatedTimeInMinutes = totalTime,
                TotalServicesCount = serviceDetails.Count,
                TotalServiceQuantity = totalQuantity
            };
        }

        #endregion

        #region GetRecommendedPackagesAsync

        public async Task<List<MaintenancePackageSummaryDto>> GetRecommendedPackagesAsync(
            int modelId,
            int topCount = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (modelId <= 0)
                {
                    return await GetPopularPackagesAsync(topCount, cancellationToken);
                }

                // Heuristic recommendation optimized:
                // 1. Find package IDs that include services with model-specific pricing
                // 2. Add popular package IDs
                // 3. Add recent active package IDs
                // 4. Merge IDs preserving priority, then load packages with minimal includes

                // Step 1: service ids for model
                var serviceIdsForModel = await _context.ModelServicePricings
                    .Where(mp => mp.ModelId == modelId && (mp.IsActive == true || mp.IsActive == null))
                    .Select(mp => mp.ServiceId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var candidateIds = new List<int>();

                if (serviceIdsForModel.Any())
                {
                    var ids = await _context.PackageServices
                        .Where(ps => serviceIdsForModel.Contains(ps.ServiceId))
                        .Select(ps => ps.PackageId)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                    // Order these packages by popularity then created date
                    var orderedIds = await _context.MaintenancePackages
                        .Where(p => ids.Contains(p.PackageId) && p.IsActive == true)
                        .OrderByDescending(p => p.IsPopular == true)
                        .ThenByDescending(p => p.CreatedDate)
                        .Select(p => p.PackageId)
                        .ToListAsync(cancellationToken);

                    candidateIds.AddRange(orderedIds);
                }

                // Step 2: popular package ids
                if (candidateIds.Count < topCount)
                {
                    var popularIds = await _context.MaintenancePackages
                        .Where(p => p.IsActive == true && p.IsPopular == true)
                        .OrderByDescending(p => p.CreatedDate)
                        .Select(p => p.PackageId)
                        .ToListAsync(cancellationToken);

                    foreach (var id in popularIds)
                    {
                        if (!candidateIds.Contains(id))
                            candidateIds.Add(id);
                        if (candidateIds.Count >= topCount) break;
                    }
                }

                // Step 3: recent active packages
                if (candidateIds.Count < topCount)
                {
                    var recentIds = await _context.MaintenancePackages
                        .Where(p => p.IsActive == true && !candidateIds.Contains(p.PackageId))
                        .OrderByDescending(p => p.CreatedDate)
                        .Select(p => p.PackageId)
                        .Take(topCount - candidateIds.Count)
                        .ToListAsync(cancellationToken);

                    candidateIds.AddRange(recentIds);
                }

                if (!candidateIds.Any())
                {
                    return new List<MaintenancePackageSummaryDto>();
                }

                // Step 4: Load packages by ids with includes and preserve order
                var packages = await _context.MaintenancePackages
                    .AsNoTracking()
                    .Where(p => candidateIds.Contains(p.PackageId))
                    .Include(p => p.PackageServices)
                        .ThenInclude(ps => ps.Service)
                            .ThenInclude(s => s.Category)
                    .AsSplitQuery()
                    .ToListAsync(cancellationToken);

                // Preserve candidate order
                var orderedPackages = candidateIds
                    .Select(id => packages.FirstOrDefault(p => p.PackageId == id))
                    .Where(p => p != null)
                    .Cast<MaintenancePackage>()
                    .ToList();

                // Map to DTOs and take topCount
                var dtos = orderedPackages
                    .Select(p => MapToSummaryDto(p))
                    .Take(topCount)
                    .ToList();

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended packages for model {ModelId}", modelId);
                throw;
            }
        }

        #endregion
    }
}
