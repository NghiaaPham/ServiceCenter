using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface cho Maintenance Package
    /// Định nghĩa các methods để tương tác với database
    /// </summary>
    public interface IMaintenancePackageRepository
    {
        // ========== QUERY METHODS (SELECT) ==========

        /// <summary>
        /// Lấy danh sách packages với pagination và filter
        /// Sử dụng cho API GET /api/maintenance-packages
        /// </summary>
        /// <param name="query">Filter parameters (search, status, price range...)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PagedResult chứa danh sách MaintenancePackageSummaryDto</returns>
        Task<PagedResult<MaintenancePackageSummaryDto>> GetAllPackagesAsync(
            MaintenancePackageQueryDto query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy chi tiết 1 package theo ID (include services)
        /// Sử dụng cho API GET /api/maintenance-packages/{id}
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>MaintenancePackageResponseDto hoặc null nếu không tìm thấy</returns>
        Task<MaintenancePackageResponseDto?> GetPackageByIdAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy package theo code (unique)
        /// Sử dụng cho API GET /api/maintenance-packages/by-code/{code}
        /// </summary>
        /// <param name="packageCode">Mã package (VD: "PKG-BASIC-2025")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>MaintenancePackageResponseDto hoặc null</returns>
        Task<MaintenancePackageResponseDto?> GetPackageByCodeAsync(
            string packageCode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách packages phổ biến (IsPopularPackage = true)
        /// Sử dụng cho API GET /api/maintenance-packages/popular
        /// </summary>
        /// <param name="topCount">Số lượng packages muốn lấy (default 5)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List các packages popular</returns>
        Task<List<MaintenancePackageSummaryDto>> GetPopularPackagesAsync(
            int topCount = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách MaintenanceService trong package (entity, không map DTO)
        /// Sử dụng nội bộ để tính toán
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List các MaintenanceService entities</returns>
        Task<List<MaintenanceService>> GetServicesInPackageAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        // ========== COMMAND METHODS (INSERT/UPDATE/DELETE) ==========

        /// <summary>
        /// Tạo package mới
        /// Sử dụng cho API POST /api/maintenance-packages
        /// </summary>
        /// <param name="request">DTO chứa thông tin package mới</param>
        /// <param name="createdByUserId">ID của user tạo (Staff/Admin)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>MaintenancePackageResponseDto của package vừa tạo</returns>
        Task<MaintenancePackageResponseDto> CreatePackageAsync(
            CreateMaintenancePackageRequestDto request,
            int createdByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật package
        /// Sử dụng cho API PUT /api/maintenance-packages/{id}
        /// </summary>
        /// <param name="request">DTO chứa thông tin cập nhật</param>
        /// <param name="updatedByUserId">ID của user update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>MaintenancePackageResponseDto sau khi update</returns>
        Task<MaintenancePackageResponseDto> UpdatePackageAsync(
            UpdateMaintenancePackageRequestDto request,
            int updatedByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa package (soft delete - set status = Deleted)
        /// Sử dụng cho API DELETE /api/maintenance-packages/{id}
        /// </summary>
        /// <param name="packageId">ID của package cần xóa</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu xóa thành công, FALSE nếu không tìm thấy</returns>
        Task<bool> DeletePackageAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        // ========== VALIDATION/CHECK METHODS ==========

        /// <summary>
        /// Kiểm tra PackageCode có tồn tại chưa (để validate unique)
        /// Sử dụng trong validation khi tạo/update package
        /// </summary>
        /// <param name="packageCode">Mã package cần check</param>
        /// <param name="excludePackageId">ID của package cần exclude (khi update)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu code đã tồn tại, FALSE nếu chưa</returns>
        Task<bool> IsPackageCodeExistsAsync(
            string packageCode,
            int? excludePackageId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tính tổng giá gốc (trước discount) của package
        /// = Sum(BasePrice * Quantity) của tất cả services trong package
        /// Sử dụng để tính SavingsAmount và validate
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tổng giá gốc (VND)</returns>
        Task<decimal> CalculateOriginalPriceBeforeDiscountAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra xem package có subscription nào đang active không
        /// Sử dụng trước khi xóa package
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu có subscription active, FALSE nếu không</returns>
        Task<bool> HasActiveSubscriptionsAsync(
            int packageId,
            CancellationToken cancellationToken = default);
    }
}
