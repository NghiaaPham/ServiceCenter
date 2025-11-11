using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories
{
    /// <summary>
    /// Query Repository cho Maintenance Package (CQRS Pattern)
    /// Chỉ chứa READ operations - không thay đổi data
    /// Có thể optimize riêng cho query (indexing, caching, read replicas...)
    /// </summary>
    public interface IMaintenancePackageQueryRepository
    {
        /// <summary>
        /// Lấy danh sách packages với pagination và filter
        /// Support search, filter by status, price range, popularity
        /// </summary>
        /// <param name="query">Query parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paged result of package summaries</returns>
        Task<PagedResult<MaintenancePackageSummaryDto>> GetAllPackagesAsync(
            MaintenancePackageQueryDto query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy chi tiết đầy đủ của 1 package theo ID
        /// Include all related data: services, pricing, etc.
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Full package details hoặc null nếu không tìm thấy</returns>
        Task<MaintenancePackageResponseDto?> GetPackageByIdAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy package theo PackageCode (unique identifier)
        /// </summary>
        /// <param name="packageCode">Package code (VD: "PKG-BASIC-2025")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Package details hoặc null</returns>
        Task<MaintenancePackageResponseDto?> GetPackageByCodeAsync(
            string packageCode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách packages phổ biến
        /// Filter: IsPopularPackage = true AND Status = Active
        /// Sort: Có thể theo số lượng subscription, rating...
        /// </summary>
        /// <param name="topCount">Số lượng packages muốn lấy (default 5)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List popular packages</returns>
        Task<List<MaintenancePackageSummaryDto>> GetPopularPackagesAsync(
            int topCount = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách MaintenanceService entities trong package
        /// Dùng nội bộ cho business logic, không map DTO
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List MaintenanceService entities</returns>
        Task<List<MaintenanceService>> GetServicesInPackageAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tính tổng giá gốc (trước discount) của package
        /// Sum(Service.BasePrice * PackageService.Quantity)
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total original price</returns>
        Task<decimal> CalculateOriginalPriceBeforeDiscountAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra PackageCode đã tồn tại chưa
        /// Dùng để validate khi tạo/update package
        /// </summary>
        /// <param name="packageCode">Code cần check</param>
        /// <param name="excludePackageId">Exclude package này (khi update)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu code đã tồn tại</returns>
        Task<bool> IsPackageCodeExistsAsync(
            string packageCode,
            int? excludePackageId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra package có subscriptions đang active không
        /// Dùng trước khi xóa package
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu có active subscriptions</returns>
        Task<bool> HasActiveSubscriptionsAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra package có tồn tại không
        /// Simple check để validate
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu package exists</returns>
        Task<bool> PackageExistsAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy các gói khuyến nghị dựa trên modelId của xe.
        /// Hiện sử dụng heuristic: packages chứa dịch vụ có giá tùy chỉnh cho model này
        /// hoặc packages đánh dấu "IsPopular". Trả về tối đa topCount.
        /// </summary>
        Task<List<MaintenancePackageSummaryDto>> GetRecommendedPackagesAsync(
            int modelId,
            int topCount = 5,
            CancellationToken cancellationToken = default);
    }
}
