using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders;

/// <summary>
/// 🎯 SEED DATA CHO TEST AUTO-ASSIGN TECHNICIAN
/// 
/// TIÊU CHÍ CHỌN "BEST TECHNICIAN":
/// Total Score = Skills(40%) + Workload(30%) + Performance(20%) + Availability(10%)
/// 
/// Dataset này tạo 30 kỹ thuật viên chia đều cho 3 service centers:
/// - Mỗi center: 10 technicians với điểm số đa dạng
/// - Tech 1-3: High performers (80-95 điểm)
/// - Tech 4-6: Medium performers (60-75 điểm)
/// - Tech 7-8: Low performers (40-55 điểm)
/// - Tech 9-10: Edge cases (very busy / no skills)
/// </summary>
public static class TechnicianTestDataSeeder
{
    public static async Task SeedAsync(EVDbContext context)
    {
        Console.WriteLine("🔧 Checking technician test data...");
        
        // Check if already seeded
        var existingTechs = await context.Users
            .Where(u => u.Username.StartsWith("techtest"))
            .OrderBy(u => u.Username)
            .ToListAsync();

        if (existingTechs.Any())
        {
            Console.WriteLine($"⏭️  Found {existingTechs.Count} existing test technicians.");
            
            var existingTechIds = existingTechs.Select(u => u.UserId).ToList();
            
            // Check if skills are also seeded
            var skillCount = await context.EmployeeSkills
                .Where(s => existingTechIds.Contains(s.UserId))
                .CountAsync();
            
            var scheduleCount = await context.TechnicianSchedules
                .Where(s => existingTechIds.Contains(s.TechnicianId))
                .CountAsync();
                
            var ratingCount = await context.ServiceRatings
                .Where(r => r.TechnicianId.HasValue && existingTechIds.Contains(r.TechnicianId.Value))
                .CountAsync();
            
            // Check if data is complete
            bool isComplete = existingTechs.Count == 30 && skillCount > 0 && scheduleCount > 0;
            
            if (isComplete)
            {
                Console.WriteLine($"✅ Test data is complete:");
                Console.WriteLine($"   - {existingTechs.Count} technicians");
                Console.WriteLine($"   - {skillCount} skills");
                Console.WriteLine($"   - {scheduleCount} schedules");
                Console.WriteLine($"   - {ratingCount} ratings");
                Console.WriteLine("⏭️  Skipping seeder...");
                return;
            }
            
            // Data is incomplete - clean up and re-seed
            Console.WriteLine("⚠️  Incomplete data detected. Cleaning up...");
            
            // Remove orphaned schedules
            var orphanedSchedules = await context.TechnicianSchedules
                .Where(s => existingTechIds.Contains(s.TechnicianId))
                .ToListAsync();
            if (orphanedSchedules.Any())
            {
                context.TechnicianSchedules.RemoveRange(orphanedSchedules);
                Console.WriteLine($"   🗑️  Removed {orphanedSchedules.Count} orphaned schedules");
            }
            
            // Remove orphaned skills
            var orphanedSkills = await context.EmployeeSkills
                .Where(s => existingTechIds.Contains(s.UserId))
                .ToListAsync();
            if (orphanedSkills.Any())
            {
                context.EmployeeSkills.RemoveRange(orphanedSkills);
                Console.WriteLine($"   🗑️  Removed {orphanedSkills.Count} orphaned skills");
            }
            
            // Remove orphaned ratings
            var orphanedRatings = await context.ServiceRatings
                .Where(r => r.TechnicianId.HasValue && existingTechIds.Contains(r.TechnicianId.Value))
                .ToListAsync();
            if (orphanedRatings.Any())
            {
                context.ServiceRatings.RemoveRange(orphanedRatings);
                Console.WriteLine($"   🗑️  Removed {orphanedRatings.Count} orphaned ratings");
            }
            
            // Remove old technician users
            context.Users.RemoveRange(existingTechs);
            Console.WriteLine($"   🗑️  Removed {existingTechs.Count} old technician users");
            
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Cleanup completed. Proceeding with fresh seed...");
        }

        Console.WriteLine("🔧 Seeding 30 test technicians (10 per service center)...");

        var (passwordHash, passwordSalt) = CreatePasswordHash("Tech@123");
        var allTechs = new List<User>();

        // Tạo 30 technicians với pattern lặp lại
        for (int i = 1; i <= 30; i++)
        {
            var techNumber = i.ToString("D3");
            var groupIndex = (i - 1) % 10; // 0-9 pattern repeats

            allTechs.Add(new User
            {
                Username = $"techtest{techNumber}",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FullName = GetTechnicianName(groupIndex, i),
                Email = $"techtest{techNumber}@evservice.com",
                PhoneNumber = $"0911{i:D6}",
                RoleId = (int)UserRoles.Technician,
                EmployeeCode = $"TEST-TECH-{techNumber}",
                Department = groupIndex % 2 == 0 ? "Maintenance" : "Diagnostics",
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5 + (groupIndex % 5))),
                Salary = 20000000 - (groupIndex * 1000000),
                IsActive = groupIndex != 8, // Tech009, 019, 029 inactive
                EmailVerified = true,
                CreatedDate = DateTime.UtcNow.AddYears(-5 + (groupIndex % 5))
            });
        }

        await context.Users.AddRangeAsync(allTechs);
        await context.SaveChangesAsync();

        var techIds = await context.Users
            .Where(u => u.Username.StartsWith("techtest"))
            .OrderBy(u => u.Username)
            .Select(u => u.UserId)
            .ToListAsync();

        Console.WriteLine($"✅ Created {techIds.Count} test technicians");

        await SeedTechnicianSkillsAsync(context, techIds);
        await SeedTechnicianSchedulesAsync(context, techIds);
        await SeedPerformanceRatingsAsync(context, techIds);

        Console.WriteLine("🎉 Test technician data seeded successfully!");
        Console.WriteLine("📝 Login: techtest001-030 / Tech@123");
        Console.WriteLine();
        PrintScoringGuide();
    }

    private static string GetTechnicianName(int groupIndex, int techNumber)
    {
        var names = new[]
        {
            $"Nguyễn Văn Tài #{techNumber} - Siêu Sao 🌟",
            $"Trần Minh Khôi #{techNumber} - Chuyên Gia Pin 🔋",
            $"Lê Quang Huy #{techNumber} - Thợ Vàng ⚙️",
            $"Phạm Thanh Long #{techNumber} - Bận Rộn 🏃",
            $"Võ Đức An #{techNumber} - Trung Bình Khá 👌",
            $"Đặng Hoàng Nam #{techNumber} - Mới Vào Nghề 🆕",
            $"Huỳnh Văn Bình #{techNumber} - Quá Tải 🔥",
            $"Bùi Công Phương #{techNumber} - Đang Học Hỏi 📚",
            $"Ngô Quốc Anh #{techNumber} - Nghỉ Phép 🏖️",
            $"Phan Minh Tuấn #{techNumber} - Không Kỹ Năng ❌"
        };
        return names[groupIndex];
    }

    /// <summary>
    /// Seed kỹ năng cho từng technician theo profile (repeating pattern)
    /// </summary>
    private static async Task SeedTechnicianSkillsAsync(EVDbContext context, List<int> techIds)
    {
        try
        {
            Console.WriteLine("🔧 Seeding technician skills...");

            var skillData = new Dictionary<int, (string[] skills, int proficiency)>
            {
                [0] = (new[] { "Battery Diagnostics", "Motor Repair", "Charging System", "Electrical Systems", "Software Updates" }, 95),
                [1] = (new[] { "Battery Diagnostics", "Thermal Management", "Charging System", "Energy Efficiency" }, 90),
                [2] = (new[] { "Motor Repair", "Transmission", "Brake System", "Suspension" }, 88),
                [3] = (new[] { "Battery Diagnostics", "Charging System", "Basic Maintenance" }, 75),
                [4] = (new[] { "Motor Repair", "Electrical Systems", "Diagnostics" }, 70),
                [5] = (new[] { "Basic Maintenance", "Battery Diagnostics" }, 65),
                [6] = (new[] { "Basic Maintenance" }, 60),
                [7] = (new[] { "Basic Maintenance", "Cleaning" }, 55),
                [8] = (new[] { "Battery Diagnostics", "Motor Repair" }, 80),
                [9] = (new string[] { }, 0)
            };

            var allSkills = new List<EmployeeSkill>();

            for (int i = 0; i < techIds.Count; i++)
            {
                var patternIndex = i % 10; // Repeat pattern every 10 techs
                
                if (skillData.TryGetValue(patternIndex, out var data))
                {
                    foreach (var skillName in data.skills)
                    {
                        allSkills.Add(new EmployeeSkill
                        {
                            UserId = techIds[i],
                            SkillName = skillName,
                            SkillLevel = data.proficiency >= 80 ? "Expert" : data.proficiency >= 60 ? "Advanced" : "Intermediate",
                            CertificationDate = patternIndex < 3 ? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)) : null,
                            ExpiryDate = patternIndex < 3 ? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)) : null,
                            CertifyingBody = patternIndex < 3 ? "EV Service Certification Board" : null,
                            CertificationNumber = patternIndex < 3 ? $"CERT-{skillName.Replace(" ", "")}-{techIds[i]}" : null,
                            IsVerified = patternIndex < 6,
                            VerifiedBy = null,
                            VerifiedDate = patternIndex < 6 ? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)) : null,
                            Notes = patternIndex < 3 ? "Excellent skills, highly recommended" : null
                        });
                    }
                }
            }

            if (allSkills.Count > 0)
            {
                await context.EmployeeSkills.AddRangeAsync(allSkills);
                await context.SaveChangesAsync();
                Console.WriteLine($"  ✅ Added {allSkills.Count} skills for {techIds.Count} technicians");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error seeding skills: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Seed schedule cho TẤT CẢ 30 technicians across ALL service centers với 7 ngày coverage
    /// </summary>
    private static async Task SeedTechnicianSchedulesAsync(EVDbContext context, List<int> techIds)
    {
        try
        {
            Console.WriteLine("📅 Seeding technician schedules...");

            var availableCenters = await context.ServiceCenters
                .Where(sc => sc.IsActive == true)
                .Select(sc => new { sc.CenterId, sc.CenterName })
                .ToListAsync();

            if (availableCenters.Count == 0)
            {
                Console.WriteLine($"  ⚠️  No active ServiceCenters found. Skipping schedules...");
                return;
            }

            Console.WriteLine($"  📍 Found {availableCenters.Count} active service centers");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var schedules = new List<TechnicianSchedule>();

            var workloadMap = new Dictionary<int, int>
            {
                [0] = 0, [1] = 1, [2] = 2, [3] = 3, [4] = 2,
                [5] = 1, [6] = 5, [7] = 4, [8] = 1, [9] = 0
            };

            // Chia đều technicians cho centers
            var techsPerCenter = techIds.Count / availableCenters.Count;
            
            for (int centerIdx = 0; centerIdx < availableCenters.Count; centerIdx++)
            {
                var center = availableCenters[centerIdx];
                var startIdx = centerIdx * techsPerCenter;
                var endIdx = (centerIdx == availableCenters.Count - 1) ? techIds.Count : (centerIdx + 1) * techsPerCenter;
                
                Console.WriteLine($"  📍 Center {center.CenterName}: Assigning {endIdx - startIdx} technicians");

                for (int i = startIdx; i < endIdx; i++)
                {
                    var patternIndex = i % 10;
                    var bookedMinutes = workloadMap.GetValueOrDefault(patternIndex, 0) * 60;

                    // 7 ngày coverage
                    for (int dayOffset = 0; dayOffset < 7; dayOffset++)
                    {
                        schedules.Add(new TechnicianSchedule
                        {
                            TechnicianId = techIds[i],
                            CenterId = center.CenterId,
                            WorkDate = today.AddDays(dayOffset),
                            StartTime = new TimeOnly(8, 0),
                            EndTime = new TimeOnly(17, 0),
                            BreakStartTime = new TimeOnly(12, 0),
                            BreakEndTime = new TimeOnly(13, 0),
                            MaxCapacityMinutes = 480,
                            BookedMinutes = bookedMinutes,
                            AvailableMinutes = 480 - bookedMinutes,
                            IsAvailable = patternIndex != 8,
                            ShiftType = "Day",
                            Notes = patternIndex switch
                            {
                                0 => "Available all day - Priority assignment",
                                6 => "Overloaded - Do not assign more work",
                                8 => "On leave - Unavailable",
                                _ => null
                            },
                            CreatedDate = DateTime.UtcNow.AddDays(-7)
                        });
                    }
                }
            }

            await context.TechnicianSchedules.AddRangeAsync(schedules);
            await context.SaveChangesAsync();
            Console.WriteLine($"  ✅ Added {schedules.Count} schedules across {availableCenters.Count} centers (7 days × {techIds.Count} techs)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error seeding schedules: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Seed performance ratings để đánh giá chất lượng làm việc của technician
    /// Pattern: Tech 1-3 = Excellent, 4-6 = Good, 7-8 = Average, 9-10 = Poor/None
    /// </summary>
    private static async Task SeedPerformanceRatingsAsync(EVDbContext context, List<int> techIds)
    {
        try
        {
            Console.WriteLine("⭐ Seeding technician performance ratings...");

            // Lấy danh sách work orders để tạo ratings (giả lập từ quá khứ)
            var workOrders = await context.WorkOrders
                .Where(wo => wo.StatusId == (int)WorkOrderStatusEnum.Completed)
                .OrderBy(wo => wo.CompletedDate)
                .Take(50) // Lấy 50 work orders gần nhất
                .Select(wo => new { wo.WorkOrderId, wo.CustomerId, wo.TechnicianId })
                .ToListAsync();

            if (!workOrders.Any())
            {
                Console.WriteLine("  ⚠️  No completed work orders found. Creating sample ratings...");
            }

            var ratings = new List<ServiceRating>();
            var performanceScores = new Dictionary<int, double>
            {
                [0] = 4.8, [1] = 4.6, [2] = 4.5, // High performers
                [3] = 4.0, [4] = 3.8, [5] = 3.5, // Medium performers
                [6] = 3.0, [7] = 2.8,             // Low performers
                [8] = 4.2, [9] = 0.0              // Edge cases
            };

            for (int i = 0; i < techIds.Count; i++)
            {
                var techId = techIds[i];
                var patternIndex = i % 10;
                var avgScore = performanceScores.GetValueOrDefault(patternIndex, 3.0);
                
                // Tạo 3-5 ratings mẫu cho mỗi tech
                int ratingCount = patternIndex switch
                {
                    0 or 1 or 2 => 5, // High performers có nhiều rating
                    3 or 4 or 5 => 3,
                    6 or 7 => 2,
                    9 => 0, // Không có kỹ năng = không có rating
                    _ => 1
                };

                for (int r = 0; r < ratingCount; r++)
                {
                    var overallRating = Math.Max(1, Math.Min(5, (int)Math.Round(avgScore + (Random.Shared.NextDouble() - 0.5))));
                    
                    // Determine feedback messages based on rating
                    string positiveFeedback = overallRating >= 4 
                        ? "Excellent service, very professional!" 
                        : (overallRating >= 3 
                            ? "Good work, satisfied with the service" 
                            : "Average service, room for improvement");
                    
                    string? negativeFeedback = overallRating < 3 
                        ? "Slow service, needs improvement" 
                        : null;
                    
                    ratings.Add(new ServiceRating
                    {
                        WorkOrderId = workOrders.Any() ? workOrders[Random.Shared.Next(workOrders.Count)].WorkOrderId : 1,
                        CustomerId = workOrders.Any() ? workOrders[Random.Shared.Next(workOrders.Count)].CustomerId : 1,
                        TechnicianId = techId,
                        OverallRating = overallRating,
                        ServiceQuality = overallRating,
                        StaffProfessionalism = Math.Max(1, Math.Min(5, overallRating + Random.Shared.Next(-1, 2))),
                        FacilityQuality = overallRating,
                        WaitingTime = Math.Max(1, Math.Min(5, overallRating + Random.Shared.Next(-1, 2))),
                        PriceValue = overallRating,
                        CommunicationQuality = Math.Max(1, Math.Min(5, overallRating + Random.Shared.Next(-1, 2))),
                        PositiveFeedback = positiveFeedback,
                        NegativeFeedback = negativeFeedback,
                        WouldRecommend = overallRating >= 4,
                        WouldReturn = overallRating >= 3,
                        RatingDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 90)),
                        IsVerified = true
                    });
                }
            }

            if (ratings.Count > 0)
            {
                await context.ServiceRatings.AddRangeAsync(ratings);
                await context.SaveChangesAsync();
                Console.WriteLine($"  ✅ Added {ratings.Count} performance ratings for {techIds.Count} technicians");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error seeding ratings: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Create password hash and salt using BCrypt (matches authentication system)
    /// </summary>
    private static (byte[] hash, byte[] salt) CreatePasswordHash(string password)
    {
        // Use SecurityHelper (BCrypt) to match the actual authentication system
        var salt = SecurityHelper.GenerateSalt(); // BCrypt salt string (~29 chars)
        var hash = SecurityHelper.HashPassword(password, salt); // BCrypt hash string (~60 chars)
        
        // Convert to bytes for storage
        var hashBytes = Encoding.UTF8.GetBytes(hash);
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        
        // Validate lengths match database constraints
        if (hashBytes.Length > 64)
            throw new InvalidOperationException($"Password hash exceeds VARBINARY(64) limit. Length: {hashBytes.Length}");
        if (saltBytes.Length > 32)
            throw new InvalidOperationException($"Password salt exceeds VARBINARY(32) limit. Length: {saltBytes.Length}");
        
        return (hashBytes, saltBytes);
    }

    /// <summary>
    /// Print scoring guide for manual testing
    /// </summary>
    private static void PrintScoringGuide()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        🎯 AUTO-ASSIGN TECHNICIAN SCORING GUIDE                     ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ Formula: Score = Skills(40%) + Workload(30%) + Performance(20%)  ║");
        Console.WriteLine("║                  + Availability(10%)                               ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ Pattern Index | Technician Type        | Expected Score Range     ║");
        Console.WriteLine("╠═══════════════╪════════════════════════╪═════════════════════════╣");
        Console.WriteLine("║ 0 (001,011..) │ Siêu Sao 🌟           │ 85-95  (High priority)  ║");
        Console.WriteLine("║ 1 (002,012..) │ Chuyên Gia Pin 🔋     │ 82-92  (High priority)  ║");
        Console.WriteLine("║ 2 (003,013..) │ Thợ Vàng ⚙️            │ 80-88  (High priority)  ║");
        Console.WriteLine("╠═══════════════╪════════════════════════╪═════════════════════════╣");
        Console.WriteLine("║ 3 (004,014..) │ Bận Rộn 🏃            │ 60-75  (Medium - busy)  ║");
        Console.WriteLine("║ 4 (005,015..) │ Trung Bình Khá 👌     │ 60-70  (Medium)         ║");
        Console.WriteLine("║ 5 (006,016..) │ Mới Vào Nghề 🆕       │ 55-65  (Medium - new)   ║");
        Console.WriteLine("╠═══════════════╪════════════════════════╪═════════════════════════╣");
        Console.WriteLine("║ 6 (007,017..) │ Quá Tải 🔥            │ 40-55  (Low - overload) ║");
        Console.WriteLine("║ 7 (008,018..) │ Đang Học Hỏi 📚       │ 45-55  (Low - learning) ║");
        Console.WriteLine("╠═══════════════╪════════════════════════╪═════════════════════════╣");
        Console.WriteLine("║ 8 (009,019..) │ Nghỉ Phép 🏖️          │ 0      (Unavailable)    ║");
        Console.WriteLine("║ 9 (010,020..) │ Không Kỹ Năng ❌      │ 0      (No skills)      ║");
        Console.WriteLine("╚═══════════════╧════════════════════════╧═════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("📋 TESTING SCENARIOS:");
        Console.WriteLine("1. Request Battery Diagnostics → Should assign Tech 001/002 (95/90 skills)");
        Console.WriteLine("2. Request Motor Repair → Should assign Tech 001/003 (88-95 skills)");
        Console.WriteLine("3. Request at overloaded center → Should assign Tech 001/002 (low workload)");
        Console.WriteLine("4. Request with Tech 007 busy → Should skip to Tech 001/002");
        Console.WriteLine("5. Request when Tech 009 on leave → Should be excluded from candidates");
        Console.WriteLine();
    }
}
