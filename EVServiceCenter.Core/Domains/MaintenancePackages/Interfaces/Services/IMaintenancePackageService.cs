using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Services
{
    /// <summary>
    /// Service interface cho Maintenance Package
    /// Chứa business logic, validation, wrap repository calls
    /// </summary>
    public interface IMaintenancePackageService
    {
        // ========== QUERY METHODS ==========

        /// <summary>
        /// Lấy danh sách packages với filter và pagination
        /// Business layer sẽ apply thêm business rules nếu cần
        /// </summary>
        Task<PagedResult<MaintenancePackageSummaryDto>> GetAllPackagesAsync(
            MaintenancePackageQueryDto query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy chi tiết 1 package theo ID
        /// </summary>
        Task<MaintenancePackageResponseDto?> GetPackageByIdAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy package theo code
        /// </summary>
        Task<MaintenancePackageResponseDto?> GetPackageByCodeAsync(
            string packageCode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách packages phổ biến
        /// Chỉ trả về packages với Status = Active và IsPopularPackage = true
        /// </summary>
        Task<List<MaintenancePackageSummaryDto>> GetPopularPackagesAsync(
            int topCount = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách packages khuyến nghị cho một model cụ thể
        /// Dùng cho API GET /api/maintenance-packages/recommended?modelId={modelId}
        /// </summary>
        Task<List<MaintenancePackageSummaryDto>> GetRecommendedPackagesAsync(
            int modelId,
            int topCount = 5,
            CancellationToken cancellationToken = default);

        // ========== COMMAND METHODS ==========

        /// <summary>
        /// Tạo package mới
        /// Validate:
        /// - PackageCode unique
        /// - Services phải tồn tại
        /// - TotalPriceAfterDiscount hợp lý
        /// - Discount % không vượt quá giá gốc
        /// </summary>
        Task<MaintenancePackageResponseDto> CreatePackageAsync(
            CreateMaintenancePackageRequestDto request,
            int createdByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật package
        /// Validate:
        /// - Package tồn tại
        /// - Không update nếu có subscription đang active (tùy business rule)
        /// - PackageCode unique (exclude chính nó)
        /// </summary>
        Task<MaintenancePackageResponseDto> UpdatePackageAsync(
            UpdateMaintenancePackageRequestDto request,
            int updatedByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa package (soft delete)
        /// Validate:
        /// - Package tồn tại
        /// - Không có subscription active (không thể xóa nếu đang có khách dùng)
        /// </summary>
        Task<bool> DeletePackageAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        // ========== VALIDATION/BUSINESS LOGIC METHODS ==========

        /// <summary>
        /// Kiểm tra xem có thể xóa package không
        /// FALSE nếu:
        /// - Package không tồn tại
        /// - Có subscription đang active
        /// TRUE nếu có thể xóa an toàn
        /// </summary>
        Task<bool> CanDeletePackageAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validate request tạo package
        /// Kiểm tra business rules phức tạp:
        /// - Giá gói sau discount phải < giá gốc
        /// - Discount % phù hợp với số tiền giảm
        /// - Services không trùng lặp
        /// </summary>
        Task<(bool IsValid, string? ErrorMessage)> ValidateCreatePackageRequestAsync(
            CreateMaintenancePackageRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validate request update package
        /// Tương tự ValidateCreatePackageRequestAsync nhưng check thêm:
        /// - Package phải tồn tại
        /// - Không update Package đang có subscription active (optional rule)
        /// </summary>
        Task<(bool IsValid, string? ErrorMessage)> ValidateUpdatePackageRequestAsync(
            UpdateMaintenancePackageRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
