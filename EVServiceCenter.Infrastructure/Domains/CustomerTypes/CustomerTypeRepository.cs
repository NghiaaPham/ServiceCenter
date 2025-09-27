using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Responses;
using EVServiceCenter.Core.Domains.CustomerTypes.Entities;
using EVServiceCenter.Core.Domains.CustomerTypes.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace EVServiceCenter.Infrastructure.Domains.CustomerTypes
{
    public class CustomerTypeRepository : ICustomerTypeRepository
    {
        private readonly EVDbContext _context;
        private readonly ILogger<CustomerTypeRepository> _logger;

        public CustomerTypeRepository(EVDbContext context, ILogger<CustomerTypeRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CustomerTypeResponseDto> CreateAsync(
            CreateCustomerTypeRequestDto request,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Business validation
                if (await ExistsAsync(request.TypeName, cancellationToken: cancellationToken))
                {
                    throw new InvalidOperationException($"Loại khách hàng '{request.TypeName}' đã tồn tại");
                }

                // Additional business rules validation
                ValidateDiscountPercent(request.DiscountPercent);

                var entity = new CustomerType
                {
                    TypeName = request.TypeName.Trim(),
                    DiscountPercent = request.DiscountPercent,
                    Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                    IsActive = request.IsActive
                };

                _context.CustomerTypes.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Created customer type: {TypeName} with ID: {TypeId}",
                    entity.TypeName, entity.TypeId);

                return new CustomerTypeResponseDto
                {
                    TypeId = entity.TypeId,
                    TypeName = entity.TypeName,
                    DiscountPercent = entity.DiscountPercent ?? 0,
                    Description = entity.Description,
                    IsActive = entity.IsActive ?? false,
                    CustomerCount = 0,
                    ActiveCustomerCount = 0,
                    TotalRevenueFromType = 0
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error creating customer type: {TypeName}", request.TypeName);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int typeId, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var entity = await _context.CustomerTypes
                    .FirstOrDefaultAsync(ct => ct.TypeId == typeId, cancellationToken);

                if (entity == null) return false;

                // Check business constraints
                if (await HasCustomersAsync(typeId, cancellationToken))
                {
                    throw new InvalidOperationException(
                        "Không thể xóa loại khách hàng đang có khách hàng sử dụng");
                }

                _context.CustomerTypes.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Deleted customer type: {TypeName} with ID: {TypeId}",
                    entity.TypeName, typeId);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error deleting customer type: {TypeId}", typeId);
                throw;
            }
        }

        //public async Task<bool> ExistsAsync(string typeName, int? excludeTypeId = null, CancellationToken cancellationToken = default)
        //{
        //    var query = _context.CustomerTypes
        //        .Where(ct => ct.TypeName.ToLowerInvariant() == typeName.ToLowerInvariant());

        //    if (excludeTypeId.HasValue)
        //    {
        //        query = query.Where(ct => ct.TypeId != excludeTypeId.Value);
        //    }

        //    return await query.AnyAsync(cancellationToken);
        //}

        public async Task<bool> ExistsAsync(string typeName, int? excludeTypeId = null, CancellationToken cancellationToken = default)
        {
            var normalizedTypeName = typeName.ToUpper();  // Chuẩn hóa tên ở phía client
            var query = _context.CustomerTypes
                .Where(ct => ct.TypeName.ToUpper() == normalizedTypeName);  // Sử dụng ToUpper thay vì ToLowerInvariant

            if (excludeTypeId.HasValue)
            {
                query = query.Where(ct => ct.TypeId != excludeTypeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<IEnumerable<CustomerTypeResponseDto>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.CustomerTypes
                .Where(ct => ct.IsActive == true)
                .OrderBy(ct => ct.TypeName)
                .Select(ct => new CustomerTypeResponseDto
                {
                    TypeId = ct.TypeId,
                    TypeName = ct.TypeName,
                    DiscountPercent = ct.DiscountPercent ?? 0,
                    Description = ct.Description,
                    IsActive = ct.IsActive ?? false,
                    CustomerCount = 0,
                    ActiveCustomerCount = 0,
                    TotalRevenueFromType = 0
                })
                .ToListAsync(cancellationToken);
        }

        //public async Task<Core.Domains.Shared.Models.PagedResult<CustomerTypeResponseDto>> GetAllAsync(
        //     CustomerTypeQueryDto query,
        //     CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        var baseQuery = _context.CustomerTypes.AsQueryable();

        //        // Apply filters
        //        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        //        {
        //            var searchTerm = query.SearchTerm.Trim().ToLowerInvariant();
        //            baseQuery = baseQuery.Where(ct =>
        //                ct.TypeName.ToLowerInvariant().Contains(searchTerm) ||
        //                (ct.Description != null && ct.Description.ToLowerInvariant().Contains(searchTerm)));
        //        }

        //        if (query.IsActive.HasValue)
        //        {
        //            baseQuery = baseQuery.Where(ct => ct.IsActive == query.IsActive.Value);
        //        }

        //        // Get total count before pagination
        //        var totalCount = await baseQuery.CountAsync(cancellationToken);

        //        if (totalCount == 0)
        //        {
        //            return PagedResultFactory.Empty<CustomerTypeResponseDto>(query.Page, query.PageSize);
        //        }

        //        // Apply sorting
        //        var validSortColumns = new[] { "TypeName", "DiscountPercent", "IsActive" };
        //        var sortBy = validSortColumns.Contains(query.SortBy) ? query.SortBy : "TypeName";
        //        var sortDirection = query.SortDesc ? "desc" : "asc";

        //        // Get basic data with pagination
        //        var items = await baseQuery
        //            .OrderBy($"{sortBy} {sortDirection}")
        //            .Skip((query.Page - 1) * query.PageSize)
        //            .Take(query.PageSize)
        //            .Select(ct => new CustomerTypeResponseDto
        //            {
        //                TypeId = ct.TypeId,
        //                TypeName = ct.TypeName,
        //                DiscountPercent = ct.DiscountPercent ?? 0,
        //                Description = ct.Description,
        //                IsActive = ct.IsActive ?? false,
        //                CustomerCount = 0,
        //                ActiveCustomerCount = 0,
        //                TotalRevenueFromType = 0
        //            })
        //            .ToListAsync(cancellationToken);

        //        // Load aggregations separately to avoid N+1 queries
        //        if (query.IncludeStats && items.Any())
        //        {
        //            var typeIds = items.Select(i => i.TypeId).ToList();
        //            var customerStats = await _context.Customers
        //                .Where(c => typeIds.Contains(c.TypeId ?? 0))
        //                .GroupBy(c => c.TypeId)
        //                .Select(g => new
        //                {
        //                    TypeId = g.Key,
        //                    CustomerCount = g.Count(),
        //                    ActiveCustomerCount = g.Count(c => c.IsActive == true),
        //                    TotalRevenue = g.Sum(c => c.TotalSpent ?? 0) // Fixed: Handle nullable decimal
        //                })
        //                .ToListAsync(cancellationToken);

        //            // Map stats to items
        //            foreach (var item in items)
        //            {
        //                var stats = customerStats.FirstOrDefault(s => s.TypeId == item.TypeId);
        //                if (stats != null)
        //                {
        //                    item.CustomerCount = stats.CustomerCount;
        //                    item.ActiveCustomerCount = stats.ActiveCustomerCount;
        //                    item.TotalRevenueFromType = stats.TotalRevenue;
        //                }
        //            }
        //        }

        //        return PagedResultFactory.Create(items, totalCount, query.Page, query.PageSize);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving customer types with query: {@Query}", query);
        //        throw;
        //    }
        //}

        public async Task<Core.Domains.Shared.Models.PagedResult<CustomerTypeResponseDto>> GetAllAsync(
     CustomerTypeQueryDto query,
     CancellationToken cancellationToken = default)
        {
            try
            {
                var baseQuery = _context.CustomerTypes.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    baseQuery = baseQuery.Where(ct => ct.TypeName.Contains(query.SearchTerm));
                }

                if (query.IsActive.HasValue)
                {
                    baseQuery = baseQuery.Where(ct => ct.IsActive == query.IsActive.Value);
                }

                // Sắp xếp tĩnh thay vì dynamic để tránh lỗi dịch query
                IQueryable<CustomerType> sortedQuery = query.SortDesc
                    ? query.SortBy switch
                    {
                        "TypeName" => baseQuery.OrderByDescending(ct => ct.TypeName),
                        "DiscountPercent" => baseQuery.OrderByDescending(ct => ct.DiscountPercent),
                        "IsActive" => baseQuery.OrderByDescending(ct => ct.IsActive),
                        _ => baseQuery.OrderByDescending(ct => ct.TypeName) // Default nếu SortBy không hợp lệ (dù validator đã kiểm tra)
                    }
                    : query.SortBy switch
                    {
                        "TypeName" => baseQuery.OrderBy(ct => ct.TypeName),
                        "DiscountPercent" => baseQuery.OrderBy(ct => ct.DiscountPercent),
                        "IsActive" => baseQuery.OrderBy(ct => ct.IsActive),
                        _ => baseQuery.OrderBy(ct => ct.TypeName) // Default
                    };

                var totalCount = await sortedQuery.CountAsync(cancellationToken);

                var items = await sortedQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(ct => new CustomerTypeResponseDto
                    {
                        TypeId = ct.TypeId,
                        TypeName = ct.TypeName,
                        DiscountPercent = ct.DiscountPercent ?? 0,
                        Description = ct.Description,
                        IsActive = ct.IsActive ?? false,
                        CustomerCount = 0,
                        ActiveCustomerCount = 0,
                        TotalRevenueFromType = 0
                    })
                    .ToListAsync(cancellationToken);

                if (query.IncludeStats)
                {
                    var typeIds = items.Select(i => i.TypeId).ToList();

                    var customerStats = await _context.Customers
                        .Where(c => typeIds.Contains(c.TypeId ?? 0))
                        .GroupBy(c => c.TypeId)
                        .Select(g => new
                        {
                            TypeId = g.Key,
                            CustomerCount = g.Count(),
                            ActiveCustomerCount = g.Count(c => c.IsActive == true),
                            TotalRevenue = g.Sum(c => c.TotalSpent ?? 0)
                        })
                        .ToListAsync(cancellationToken);

                    foreach (var item in items)
                    {
                        var stats = customerStats.FirstOrDefault(s => s.TypeId == item.TypeId);
                        if (stats != null)
                        {
                            item.CustomerCount = stats.CustomerCount;
                            item.ActiveCustomerCount = stats.ActiveCustomerCount;
                            item.TotalRevenueFromType = stats.TotalRevenue;
                        }
                    }
                }

                return PagedResultFactory.Create(items, totalCount, query.Page, query.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer types with query: {@Query}", query);
                throw;
            }
        }

        public async Task<CustomerTypeResponseDto?> GetByIdAsync(int typeId, bool includeStats = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var customerType = await _context.CustomerTypes
                    .Where(ct => ct.TypeId == typeId)
                    .Select(ct => new CustomerTypeResponseDto
                    {
                        TypeId = ct.TypeId,
                        TypeName = ct.TypeName,
                        DiscountPercent = ct.DiscountPercent ?? 0,
                        Description = ct.Description,
                        IsActive = ct.IsActive ?? false,
                        CustomerCount = 0,
                        ActiveCustomerCount = 0,
                        TotalRevenueFromType = 0
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerType == null) return null;

                // Load stats separately if requested
                if (includeStats)
                {
                    var customerStats = await _context.Customers
                        .Where(c => c.TypeId == typeId)
                        .GroupBy(c => 1)
                        .Select(g => new
                        {
                            CustomerCount = g.Count(),
                            ActiveCustomerCount = g.Count(c => c.IsActive == true),
                            TotalRevenue = g.Sum(c => c.TotalSpent ?? 0) // Fixed: Handle nullable decimal
                        })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (customerStats != null)
                    {
                        customerType.CustomerCount = customerStats.CustomerCount;
                        customerType.ActiveCustomerCount = customerStats.ActiveCustomerCount;
                        customerType.TotalRevenueFromType = customerStats.TotalRevenue;
                    }
                }
                return customerType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer type by ID: {TypeId}", typeId);
                throw;
            }
        }
        public async Task<bool> HasCustomersAsync(int typeId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .AnyAsync(c => c.TypeId == typeId, cancellationToken);
        }

        public async Task<CustomerTypeResponseDto> UpdateAsync(
            UpdateCustomerTypeRequestDto request,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var entity = await _context.CustomerTypes
                    .FirstOrDefaultAsync(ct => ct.TypeId == request.TypeId, cancellationToken);

                if (entity == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy loại khách hàng với ID {request.TypeId}");
                }

                // Business validation
                if (await ExistsAsync(request.TypeName, request.TypeId, cancellationToken))
                {
                    throw new InvalidOperationException($"Loại khách hàng '{request.TypeName}' đã tồn tại");
                }

                ValidateDiscountPercent(request.DiscountPercent);

                entity.TypeName = request.TypeName.Trim();
                entity.DiscountPercent = request.DiscountPercent;
                entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
                entity.IsActive = request.IsActive;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Updated customer type: {TypeName} with ID: {TypeId}",
                    entity.TypeName, entity.TypeId);

                return await GetByIdAsync(entity.TypeId, true, cancellationToken)
                    ?? throw new InvalidOperationException("Không thể lấy thông tin loại khách hàng sau khi cập nhật");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error updating customer type: {TypeId}", request.TypeId);
                throw;
            }
        }

        private static void ValidateDiscountPercent(decimal discountPercent)
        {
            if (discountPercent < 0 || discountPercent > 100)
            {
                throw new ArgumentException("Phần trăm giảm giá phải từ 0 đến 100");
            }
        }
    }
}