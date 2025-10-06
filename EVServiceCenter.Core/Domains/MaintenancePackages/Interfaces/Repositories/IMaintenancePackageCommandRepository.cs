using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories
{
    /// <summary>
    /// Command Repository cho Maintenance Package (CQRS Pattern)
    /// Chỉ chứa WRITE operations - Create, Update, Delete
    /// Thay đổi state của database
    /// </summary>
    public interface IMaintenancePackageCommandRepository
    {
        /// <summary>
        /// Tạo package mới
        /// Flow:
        /// 1. Create MaintenancePackage entity
        /// 2. Create PackageService relations (link package với services)
        /// 3. Save changes
        /// 4. Return full package details
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
        /// Flow:
        /// 1. Load existing package entity
        /// 2. Update package properties
        /// 3. Delete old PackageService relations
        /// 4. Create new PackageService relations (từ request.IncludedServices)
        /// 5. Save changes
        /// 6. Return updated package details
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
        /// Xóa package (soft delete)
        /// Set Status = Deleted, không xóa khỏi database
        /// Giữ lại để tham chiếu với subscriptions cũ
        /// </summary>
        /// <param name="packageId">ID của package cần xóa</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu xóa thành công, FALSE nếu không tìm thấy</returns>
        Task<bool> SoftDeletePackageAsync(
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Thay đổi trạng thái package (Active/Inactive)
        /// VD: Tạm ngưng bán gói (Inactive), mở lại (Active)
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="newStatus">Trạng thái mới (Active/Inactive)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu update thành công</returns>
        Task<bool> UpdatePackageStatusAsync(
            int packageId,
            Core.Enums.PackageStatusEnum newStatus,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Toggle IsPopularPackage flag
        /// Đánh dấu/bỏ đánh dấu gói là popular
        /// </summary>
        /// <param name="packageId">ID của package</param>
        /// <param name="isPopular">TRUE = đánh dấu popular, FALSE = bỏ đánh dấu</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu update thành công</returns>
        Task<bool> SetPackagePopularityAsync(
            int packageId,
            bool isPopular,
            CancellationToken cancellationToken = default);
    }
}
