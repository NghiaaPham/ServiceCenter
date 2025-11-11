using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;


namespace EVServiceCenter.Infrastructure.Domains.Customers.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly EVDbContext _context;
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(EVDbContext context, ILogger<CustomerRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<CustomerResponseDto>> GetAllAsync(CustomerQueryDto query, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseQuery = _context.Customers.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchTerm = query.SearchTerm.Trim();
                    baseQuery = baseQuery.Where(c =>
                        c.FullName.Contains(searchTerm) ||
                        c.CustomerCode.Contains(searchTerm) ||
                        c.PhoneNumber.Contains(searchTerm) ||
                        c.Email != null && c.Email.Contains(searchTerm));
                }

                if (query.TypeId.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.TypeId == query.TypeId.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.Gender))
                {
                    baseQuery = baseQuery.Where(c => c.Gender == query.Gender);
                }

                if (query.IsActive.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.IsActive == query.IsActive.Value);
                }

                if (query.MarketingOptIn.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.MarketingOptIn == query.MarketingOptIn.Value);
                }

                // Date filters
                if (query.DateOfBirthFrom.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.DateOfBirth >= query.DateOfBirthFrom.Value);
                }

                if (query.DateOfBirthTo.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.DateOfBirth <= query.DateOfBirthTo.Value);
                }

                // Financial filters
                if (query.TotalSpentFrom.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.TotalSpent >= query.TotalSpentFrom.Value);
                }

                if (query.TotalSpentTo.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.TotalSpent <= query.TotalSpentTo.Value);
                }

                // Loyalty points filters
                if (query.LoyaltyPointsFrom.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.LoyaltyPoints >= query.LoyaltyPointsFrom.Value);
                }

                if (query.LoyaltyPointsTo.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.LoyaltyPoints <= query.LoyaltyPointsTo.Value);
                }

                // Visit date filters
                if (query.LastVisitFrom.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.LastVisitDate >= query.LastVisitFrom.Value);
                }

                if (query.LastVisitTo.HasValue)
                {
                    baseQuery = baseQuery.Where(c => c.LastVisitDate <= query.LastVisitTo.Value);
                }

                // Apply sorting
                IQueryable<Customer> sortedQuery = query.SortDesc
                    ? query.SortBy switch
                    {
                        "FullName" => baseQuery.OrderByDescending(c => c.FullName),
                        "CustomerCode" => baseQuery.OrderByDescending(c => c.CustomerCode),
                        "CreatedDate" => baseQuery.OrderByDescending(c => c.CreatedDate),
                        "LastVisitDate" => baseQuery.OrderByDescending(c => c.LastVisitDate),
                        "TotalSpent" => baseQuery.OrderByDescending(c => c.TotalSpent),
                        "LoyaltyPoints" => baseQuery.OrderByDescending(c => c.LoyaltyPoints),
                        "PhoneNumber" => baseQuery.OrderByDescending(c => c.PhoneNumber),
                        "Email" => baseQuery.OrderByDescending(c => c.Email),
                        _ => baseQuery.OrderByDescending(c => c.FullName)
                    }
                    : query.SortBy switch
                    {
                        "FullName" => baseQuery.OrderBy(c => c.FullName),
                        "CustomerCode" => baseQuery.OrderBy(c => c.CustomerCode),
                        "CreatedDate" => baseQuery.OrderBy(c => c.CreatedDate),
                        "LastVisitDate" => baseQuery.OrderBy(c => c.LastVisitDate),
                        "TotalSpent" => baseQuery.OrderBy(c => c.TotalSpent),
                        "LoyaltyPoints" => baseQuery.OrderBy(c => c.LoyaltyPoints),
                        "PhoneNumber" => baseQuery.OrderBy(c => c.PhoneNumber),
                        "Email" => baseQuery.OrderBy(c => c.Email),
                        _ => baseQuery.OrderBy(c => c.FullName)
                    };

                var totalCount = await sortedQuery.CountAsync(cancellationToken);

                var items = await sortedQuery
                    .Include(c => c.Type) // Include CustomerType for navigation
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(c => new CustomerResponseDto
                    {
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber,
                        Email = c.Email,
                        Address = c.Address,
                        DateOfBirth = c.DateOfBirth,
                        Gender = c.Gender,
                        TypeId = c.TypeId,
                        PreferredLanguage = c.PreferredLanguage ?? "vi-VN",
                        MarketingOptIn = c.MarketingOptIn,
                        LoyaltyPoints = c.LoyaltyPoints,
                        TotalSpent = c.TotalSpent,
                        LastVisitDate = c.LastVisitDate,
                        Notes = c.Notes,
                        IsActive = c.IsActive,
                        CreatedDate = c.CreatedDate,
                        CustomerType = c.Type != null ? new CustomerTypeResponseDto
                        {
                            TypeId = c.Type.TypeId,
                            TypeName = c.Type.TypeName,
                            DiscountPercent = c.Type.DiscountPercent ?? 0,
                            Description = c.Type.Description,
                            IsActive = c.Type.IsActive ?? false
                        } : null,
                        // Computed properties
                        Age = c.DateOfBirth.HasValue ?
                            DateTime.Today.Year - c.DateOfBirth.Value.Year -
                            (DateTime.Today.DayOfYear < c.DateOfBirth.Value.DayOfYear ? 1 : 0) : 0,
                        DisplayName = c.CustomerCode + " - " + c.FullName,
                        ContactInfo = !string.IsNullOrEmpty(c.Email) ? c.PhoneNumber + " / " + c.Email : c.PhoneNumber,
                        LoyaltyStatus = GetLoyaltyStatus(c.LoyaltyPoints ?? 0),
                        PotentialDiscount = c.Type != null ? c.Type.DiscountPercent ?? 0 : 0,
                        LastVisitStatus = GetLastVisitStatus(c.LastVisitDate),
                        VehicleCount = 0, // Will be populated separately if needed
                        ActiveVehicleCount = 0,
                        RecentVehicles = new List<CustomerVehicleSummaryDto>()
                    })
                    .ToListAsync(cancellationToken);

                // Load vehicle stats separately if requested
                if (query.IncludeStats && items.Any())
                {
                    var customerIds = items.Select(c => c.CustomerId).ToList();
                    var vehicleStats = await _context.CustomerVehicles
                        .Where(v => customerIds.Contains(v.CustomerId))
                        .GroupBy(v => v.CustomerId)
                        .Select(g => new
                        {
                            CustomerId = g.Key,
                            VehicleCount = g.Count(),
                            ActiveVehicleCount = g.Count(v => v.IsActive == true),
                            RecentVehicles = g.Where(v => v.IsActive == true)
                                .OrderByDescending(v => v.CreatedDate)
                                .Take(3)
                                .Select(v => new CustomerVehicleSummaryDto
                                {
                                    VehicleId = v.VehicleId,
                                    LicensePlate = v.LicensePlate,
                                    ModelName = v.Model.ModelName,
                                    BrandName = v.Model.Brand.BrandName,
                                    IsActive = v.IsActive ?? false,
                                    IsMaintenanceDue = v.NextMaintenanceDate.HasValue && v.NextMaintenanceDate <= DateOnly.FromDateTime(DateTime.Today) ||
                                                      v.NextMaintenanceMileage.HasValue && v.Mileage >= v.NextMaintenanceMileage
                                }).ToList()
                        })
                        .ToListAsync(cancellationToken);

                    foreach (var item in items)
                    {
                        var stats = vehicleStats.FirstOrDefault(s => s.CustomerId == item.CustomerId);
                        if (stats != null)
                        {
                            item.VehicleCount = stats.VehicleCount;
                            item.ActiveVehicleCount = stats.ActiveVehicleCount;
                            item.RecentVehicles = stats.RecentVehicles;
                        }
                    }
                }

                return PagedResultFactory.Create(items, totalCount, query.Page, query.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers with query: {@Query}", query);
                throw;
            }
        }

        public async Task<CustomerResponseDto?> GetByIdAsync(int customerId, bool includeStats = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Type)
                    .Where(c => c.CustomerId == customerId)
                    .Select(c => new CustomerResponseDto
                    {
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber,
                        Email = c.Email,
                        Address = c.Address,
                        DateOfBirth = c.DateOfBirth,
                        Gender = c.Gender,
                        TypeId = c.TypeId,
                        PreferredLanguage = c.PreferredLanguage ?? "vi-VN",
                        MarketingOptIn = c.MarketingOptIn,
                        LoyaltyPoints = c.LoyaltyPoints,
                        TotalSpent = c.TotalSpent,
                        LastVisitDate = c.LastVisitDate,
                        Notes = c.Notes,
                        IsActive = c.IsActive,
                        CreatedDate = c.CreatedDate,
                        CustomerType = c.Type != null ? new CustomerTypeResponseDto
                        {
                            TypeId = c.Type.TypeId,
                            TypeName = c.Type.TypeName,
                            DiscountPercent = c.Type.DiscountPercent ?? 0,
                            Description = c.Type.Description,
                            IsActive = c.Type.IsActive ?? false
                        } : null,
                        Age = c.DateOfBirth.HasValue ?
                            DateTime.Today.Year - c.DateOfBirth.Value.Year -
                            (DateTime.Today.DayOfYear < c.DateOfBirth.Value.DayOfYear ? 1 : 0) : 0,
                        DisplayName = c.CustomerCode + " - " + c.FullName,
                        ContactInfo = !string.IsNullOrEmpty(c.Email) ? c.PhoneNumber + " / " + c.Email : c.PhoneNumber,
                        LoyaltyStatus = GetLoyaltyStatus(c.LoyaltyPoints ?? 0),
                        PotentialDiscount = c.Type != null ? c.Type.DiscountPercent ?? 0 : 0,
                        LastVisitStatus = GetLastVisitStatus(c.LastVisitDate),
                        VehicleCount = 0,
                        ActiveVehicleCount = 0,
                        RecentVehicles = new List<CustomerVehicleSummaryDto>()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (customer == null) return null;

                // Load vehicle stats if requested
                if (includeStats)
                {
                    var vehicleStats = await _context.CustomerVehicles
                        .Where(v => v.CustomerId == customerId)
                        .Include(v => v.Model)
                        .ThenInclude(m => m.Brand)
                        .GroupBy(v => v.CustomerId)
                        .Select(g => new
                        {
                            VehicleCount = g.Count(),
                            ActiveVehicleCount = g.Count(v => v.IsActive == true),
                            RecentVehicles = g.Where(v => v.IsActive == true)
                                .OrderByDescending(v => v.CreatedDate)
                                .Take(5)
                                .Select(v => new CustomerVehicleSummaryDto
                                {
                                    VehicleId = v.VehicleId,
                                    LicensePlate = v.LicensePlate,
                                    ModelName = v.Model.ModelName,
                                    BrandName = v.Model.Brand.BrandName,
                                    IsActive = v.IsActive ?? false,
                                    IsMaintenanceDue = v.NextMaintenanceDate.HasValue && v.NextMaintenanceDate <= DateOnly.FromDateTime(DateTime.Today) ||
                                                      v.NextMaintenanceMileage.HasValue && v.Mileage >= v.NextMaintenanceMileage
                                }).ToList()
                        })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (vehicleStats != null)
                    {
                        customer.VehicleCount = vehicleStats.VehicleCount;
                        customer.ActiveVehicleCount = vehicleStats.ActiveVehicleCount;
                        customer.RecentVehicles = vehicleStats.RecentVehicles;
                    }
                }

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerResponseDto?> GetByCustomerCodeAsync(string customerCode, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.Type)
                    .Where(c => c.CustomerCode == customerCode)
                    .Select(c => new CustomerResponseDto
                    {
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber,
                        Email = c.Email,
                        DisplayName = c.CustomerCode + " - " + c.FullName,
                        ContactInfo = !string.IsNullOrEmpty(c.Email) ? c.PhoneNumber + " / " + c.Email : c.PhoneNumber,
                        IsActive = c.IsActive
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by code: {CustomerCode}", customerCode);
                throw;
            }
        }

        public async Task<CustomerResponseDto?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.Type)
                    .Where(c => c.PhoneNumber == phoneNumber)
                    .Select(c => new CustomerResponseDto
                    {
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber,
                        Email = c.Email,
                        DisplayName = c.CustomerCode + " - " + c.FullName,
                        ContactInfo = !string.IsNullOrEmpty(c.Email) ? c.PhoneNumber + " / " + c.Email : c.PhoneNumber,
                        IsActive = c.IsActive
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by phone: {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task<CustomerResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Customers
                    .Include(c => c.Type)
                    .Where(c => c.Email == email)
                    .Select(c => new CustomerResponseDto
                    {
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        FullName = c.FullName,
                        PhoneNumber = c.PhoneNumber,
                        Email = c.Email,
                        DisplayName = c.CustomerCode + " - " + c.FullName,
                        ContactInfo = !string.IsNullOrEmpty(c.Email) ? c.PhoneNumber + " / " + c.Email : c.PhoneNumber,
                        IsActive = c.IsActive
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer by email: {Email}", email);
                throw;
            }
        }

        public async Task<CustomerResponseDto> UpdateAsync(UpdateCustomerRequestDto request, CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Use ExecutionStrategy to handle transaction with retry logic
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var entity = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

                    if (entity == null)
                    {
                        throw new InvalidOperationException($"Không tìm thấy khách hàng với ID {request.CustomerId}");
                    }

                    // Business validation
                    if (await ExistsAsync(request.PhoneNumber, request.CustomerId, cancellationToken))
                    {
                        throw new InvalidOperationException($"Số điện thoại '{request.PhoneNumber}' đã được sử dụng bởi khách hàng khác");
                    }

                    if (!string.IsNullOrWhiteSpace(request.Email) && await EmailExistsAsync(request.Email, request.CustomerId, cancellationToken))
                    {
                        throw new InvalidOperationException($"Email '{request.Email}' đã được sử dụng bởi khách hàng khác");
                    }

                    entity.FullName = request.FullName.Trim();
                    entity.PhoneNumber = request.PhoneNumber.Trim();
                    entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
                    entity.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
                    entity.DateOfBirth = request.DateOfBirth;
                    entity.Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
                    entity.TypeId = request.TypeId ?? entity.TypeId;
                    entity.PreferredLanguage = request.PreferredLanguage;
                    entity.MarketingOptIn = request.MarketingOptIn;
                    entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
                    entity.IsActive = request.IsActive;
                    entity.UpdatedDate = DateTime.UtcNow;

                    // ✅ FIX: Handle identity number update ONLY if provided
                    if (request.IdentityNumber != null)
                    {
                        if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
                        {
                            if (await IdentityNumberExistsAsync(request.IdentityNumber, request.CustomerId, cancellationToken))
                            {
                                throw new InvalidOperationException("Số CMND/CCCD đã được sử dụng bởi khách hàng khác");
                            }
                            entity.IdentityNumber = EncryptIdentityNumber(request.IdentityNumber);
                        }
                        else
                        {
                            entity.IdentityNumber = null;
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Updated customer: {CustomerCode} - {FullName}", entity.CustomerCode, entity.FullName);

                    return await GetByIdAsync(entity.CustomerId, false, cancellationToken)
                        ?? throw new InvalidOperationException("Failed to retrieve updated customer");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error updating customer: {CustomerId}", request.CustomerId);
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAsync(int customerId, CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Use ExecutionStrategy
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var entity = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

                    if (entity == null) return false;

                    // Check if customer has vehicles
                    if (await HasVehiclesAsync(customerId, cancellationToken))
                    {
                        throw new InvalidOperationException("Không thể xóa khách hàng đang có xe trong hệ thống");
                    }

                    _context.Customers.Remove(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Deleted customer: {CustomerCode} - {FullName}", entity.CustomerCode, entity.FullName);

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error deleting customer: {CustomerId}", customerId);
                    throw;
                }
            });
        }
        public async Task<string> GenerateCustomerCodeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<string>("EXEC sp_GetNextCustomerCode")
                    .ToListAsync(cancellationToken);

                return result.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer code with sequence");
                throw;
            }
        }

        // Helper methods
        private static string GetLoyaltyStatus(int loyaltyPoints)
        {
            return loyaltyPoints switch
            {
                >= 10000 => "VIP",
                >= 5000 => "Gold",
                >= 2000 => "Silver",
                >= 500 => "Bronze",
                _ => "Regular"
            };
        }

        private static string GetLastVisitStatus(DateOnly? lastVisitDate)
        {
            if (!lastVisitDate.HasValue)
                return "Chưa có lần ghé thăm";

            var daysSinceVisit = DateOnly.FromDateTime(DateTime.Today).DayNumber - lastVisitDate.Value.DayNumber;

            return daysSinceVisit switch
            {
                <= 7 => "Vừa ghé thăm",
                <= 30 => "Ghé thăm gần đây",
                <= 90 => "Lâu không ghé thăm",
                _ => "Khách hàng cũ"
            };
        }

        private static byte[] EncryptIdentityNumber(string identityNumber)
        {
            // Simple encryption - in production, use proper encryption with key management
            var bytes = Encoding.UTF8.GetBytes(identityNumber);
            return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        }

        private static string DecryptIdentityNumber(byte[] encryptedData)
        {
            // Simple decryption
            var bytes = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }

        // Validation helper methods
        public async Task<bool> ExistsAsync(string phoneNumber, int? excludeCustomerId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Customers.Where(c => c.PhoneNumber == phoneNumber);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.CustomerId != excludeCustomerId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Customers.Where(c => c.Email == email);

            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.CustomerId != excludeCustomerId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IdentityNumberExistsAsync(string identityNumber, int? excludeCustomerId = null, CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Check empty trước
            if (string.IsNullOrWhiteSpace(identityNumber))
            {
                return false; // Empty identity number không cần check duplicate
            }

            // ⚠️ LIMITATION: Encryption-based comparison
            // Vì IdentityNumber được encrypt, không thể so sánh trực tiếp trong database
            // Cần decrypt từng record để compare - KHÔNG HIỆU QUẢ với dataset lớn
            
            // Solution 1 (Current): Encrypt input và so sánh bytes - KHÔNG CHÍNH XÁC vì encryption mỗi lần khác nhau
            // Solution 2 (Recommended): Lưu hash thay vì encrypt, hoặc dùng searchable encryption
            
            // ✅ WORKAROUND: Lấy tất cả customers có IdentityNumber, decrypt và compare
            var customersWithIdentity = await _context.Customers
                .Where(c => c.IdentityNumber != null)
                .Where(c => !excludeCustomerId.HasValue || c.CustomerId != excludeCustomerId.Value)
                .Select(c => new { c.CustomerId, c.IdentityNumber })
                .ToListAsync(cancellationToken);

            // Decrypt and compare
            foreach (var customer in customersWithIdentity)
            {
                try
                {
                    var decrypted = DecryptIdentityNumber(customer.IdentityNumber!);
                    if (decrypted == identityNumber)
                    {
                        return true; // Found duplicate
                    }
                }
                catch
                {
                    // Skip if decryption fails
                    continue;
                }
            }

            return false; // No duplicate found
        }

        public async Task<bool> HasVehiclesAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await _context.CustomerVehicles
                .AnyAsync(v => v.CustomerId == customerId, cancellationToken);
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.FullName)
                .Select(c => new CustomerResponseDto
                {
                    CustomerId = c.CustomerId,
                    CustomerCode = c.CustomerCode,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    IsActive = c.IsActive,
                    DisplayName = c.CustomerCode + " - " + c.FullName,
                    ContactInfo = !string.IsNullOrEmpty(c.Email) ? c.PhoneNumber + " / " + c.Email : c.PhoneNumber
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CustomerResponseDto>> GetCustomersWithMaintenanceDueAsync(CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var upcomingDays = 30;

            return await _context.Customers
                .Where(c => c.IsActive == true && c.CustomerVehicles.Any(v =>
                    v.IsActive == true &&
                    (v.NextMaintenanceDate.HasValue && v.NextMaintenanceDate <= today.AddDays(upcomingDays) ||
                     v.NextMaintenanceMileage.HasValue && v.Mileage >= v.NextMaintenanceMileage)))
                .Include(c => c.Type)
                .Select(c => new CustomerResponseDto
                {
                    CustomerId = c.CustomerId,
                    CustomerCode = c.CustomerCode,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    LastVisitDate = c.LastVisitDate,
                    IsActive = c.IsActive,
                    DisplayName = c.CustomerCode + " - " + c.FullName,
                    ContactInfo = !string.IsNullOrEmpty(c.Email) ? c.PhoneNumber + " / " + c.Email : c.PhoneNumber,
                    VehicleCount = c.CustomerVehicles.Count(v => v.IsActive == true),
                    CustomerType = c.Type != null ? new CustomerTypeResponseDto
                    {
                        TypeId = c.Type.TypeId,
                        TypeName = c.Type.TypeName,
                        DiscountPercent = c.Type.DiscountPercent ?? 0
                    } : null
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetCustomerStatsByTypeAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Where(c => c.IsActive == true)
                .Include(c => c.Type)
                .GroupBy(c => c.Type!.TypeName)
                .Select(g => new { TypeName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TypeName, x => x.Count, cancellationToken);
        }

        public async Task<bool> UpdateLoyaltyPointsAsync(int customerId, int points, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

                if (customer == null) return false;

                customer.LoyaltyPoints = (customer.LoyaltyPoints ?? 0) + points;
                customer.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated loyalty points for customer {CustomerCode}: +{Points} points",
                    customer.CustomerCode, points);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loyalty points for customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> UpdateTotalSpentAsync(int customerId, decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

                if (customer == null) return false;

                customer.TotalSpent = (customer.TotalSpent ?? 0) + amount;
                customer.LastVisitDate = DateOnly.FromDateTime(DateTime.Today);
                customer.UpdatedDate = DateTime.UtcNow;

                // Calculate and add loyalty points (1 point per 10,000 VND spent)
                var pointsToAdd = (int)(amount / 10000);
                if (pointsToAdd > 0)
                {
                    customer.LoyaltyPoints = (customer.LoyaltyPoints ?? 0) + pointsToAdd;
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated total spent for customer {CustomerCode}: +{Amount} VND, +{Points} loyalty points",
                    customer.CustomerCode, amount, pointsToAdd);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating total spent for customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request, int? userId = null, CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Use ExecutionStrategy
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Business validation
                    if (await ExistsAsync(request.PhoneNumber, cancellationToken: cancellationToken))
                    {
                        throw new InvalidOperationException($"Số điện thoại '{request.PhoneNumber}' đã được sử dụng");
                    }

                    if (!string.IsNullOrWhiteSpace(request.Email) && await EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
                    {
                        throw new InvalidOperationException($"Email '{request.Email}' đã được sử dụng");
                    }

                    // Generate customer code
                    var customerCode = await GenerateCustomerCodeAsync(cancellationToken);

                    var entity = new Customer
                    {
                        UserId = userId,
                        CustomerCode = customerCode,
                        FullName = request.FullName.Trim(),
                        PhoneNumber = request.PhoneNumber.Trim(),
                        Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                        Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
                        DateOfBirth = request.DateOfBirth,
                        Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim(),
                        IdentityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber) ? null : EncryptIdentityNumber(request.IdentityNumber),
                        TypeId = request.TypeId ?? 1,
                        PreferredLanguage = request.PreferredLanguage,
                        MarketingOptIn = request.MarketingOptIn,
                        LoyaltyPoints = 0,
                        TotalSpent = 0,
                        Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                        IsActive = request.IsActive,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Customers.Add(entity);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Created customer: {CustomerCode} - {FullName}", entity.CustomerCode, entity.FullName);

                    return await GetByIdAsync(entity.CustomerId, false, cancellationToken)
                        ?? throw new InvalidOperationException("Failed to retrieve created customer");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error creating customer: {FullName}", request.FullName);
                    throw;
                }
            });
        }
    }
}
