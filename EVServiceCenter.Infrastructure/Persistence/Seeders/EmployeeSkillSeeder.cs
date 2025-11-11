using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder for Technician Skills
/// Creates realistic skill sets for each technician based on their experience level
/// </summary>
public static class EmployeeSkillSeeder
{
    public static void SeedEmployeeSkills(EVDbContext context)
    {
        if (context.EmployeeSkills.Any())
        {
            Console.WriteLine("Employee skills already seeded. Skipping...");
            return;
        }

        // Get all technicians
        var technicians = context.Users
            .Where(u => u.RoleId == (int)UserRoles.Technician)
            .OrderBy(u => u.UserId)
            .ToList();

        if (!technicians.Any())
        {
            Console.WriteLine("⚠️ No technicians found. Please seed technicians first.");
            return;
        }

        // Get an admin to verify skills
        var verifier = context.Users
            .FirstOrDefault(u => u.RoleId == (int)UserRoles.Admin);

        var skills = new List<EmployeeSkill>();

        // Technician 1: Senior - Expert in Battery and Diagnostics
        if (technicians.Count > 0)
        {
            var tech = technicians[0];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Battery Replacement", "Expert", -60, verifier?.UserId),
                CreateSkill(tech.UserId, "Diagnostics", "Expert", -58, verifier?.UserId),
                CreateSkill(tech.UserId, "Brake System Repair", "Intermediate", -55, verifier?.UserId),
                CreateSkill(tech.UserId, "Software Update", "Intermediate", -50, verifier?.UserId),
                CreateSkill(tech.UserId, "HVAC System", "Beginner", -40, null)
            });
        }

        // Technician 2: Senior - Expert in Motor and Diagnostics
        if (technicians.Count > 1)
        {
            var tech = technicians[1];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Motor Repair", "Expert", -48, verifier?.UserId),
                CreateSkill(tech.UserId, "Diagnostics", "Expert", -45, verifier?.UserId),
                CreateSkill(tech.UserId, "Battery Replacement", "Intermediate", -40, verifier?.UserId),
                CreateSkill(tech.UserId, "Suspension System", "Intermediate", -35, verifier?.UserId)
            });
        }

        // Technician 3: Mid-level - Strong in Diagnostics
        if (technicians.Count > 2)
        {
            var tech = technicians[2];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Diagnostics", "Expert", -36, verifier?.UserId),
                CreateSkill(tech.UserId, "Software Update", "Expert", -34, verifier?.UserId),
                CreateSkill(tech.UserId, "Brake System Repair", "Intermediate", -30, verifier?.UserId),
                CreateSkill(tech.UserId, "Battery Replacement", "Beginner", -25, null)
            });
        }

        // Technician 4: Mid-level - Balanced skill set
        if (technicians.Count > 3)
        {
            var tech = technicians[3];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Battery Replacement", "Intermediate", -24, verifier?.UserId),
                CreateSkill(tech.UserId, "Brake System Repair", "Intermediate", -22, verifier?.UserId),
                CreateSkill(tech.UserId, "Diagnostics", "Intermediate", -20, verifier?.UserId),
                CreateSkill(tech.UserId, "Tire Services", "Intermediate", -18, verifier?.UserId)
            });
        }

        // Technician 5: Mid-level - Software specialist
        if (technicians.Count > 4)
        {
            var tech = technicians[4];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Software Update", "Expert", -24, verifier?.UserId),
                CreateSkill(tech.UserId, "Diagnostics", "Expert", -22, verifier?.UserId),
                CreateSkill(tech.UserId, "Infotainment System", "Intermediate", -20, verifier?.UserId),
                CreateSkill(tech.UserId, "Battery Replacement", "Beginner", -15, null)
            });
        }

        // Technician 6: Junior - Limited skills
        if (technicians.Count > 5)
        {
            var tech = technicians[5];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Tire Services", "Intermediate", -18, verifier?.UserId),
                CreateSkill(tech.UserId, "Brake System Repair", "Beginner", -15, null),
                CreateSkill(tech.UserId, "Battery Replacement", "Beginner", -12, null),
                CreateSkill(tech.UserId, "Diagnostics", "Beginner", -10, null)
            });
        }

        // Technician 7: Senior (Service Center 2) - Battery expert
        if (technicians.Count > 6)
        {
            var tech = technicians[6];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Battery Replacement", "Expert", -36, verifier?.UserId),
                CreateSkill(tech.UserId, "Charging System", "Expert", -34, verifier?.UserId),
                CreateSkill(tech.UserId, "Diagnostics", "Intermediate", -30, verifier?.UserId),
                CreateSkill(tech.UserId, "Motor Repair", "Intermediate", -28, verifier?.UserId)
            });
        }

        // Technician 8: Mid-level (Service Center 2) - Diagnostics
        if (technicians.Count > 7)
        {
            var tech = technicians[7];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Diagnostics", "Expert", -24, verifier?.UserId),
                CreateSkill(tech.UserId, "Software Update", "Intermediate", -22, verifier?.UserId),
                CreateSkill(tech.UserId, "Battery Replacement", "Intermediate", -20, verifier?.UserId)
            });
        }

        // Technician 9: Mid-level (Service Center 2) - Mechanical
        if (technicians.Count > 8)
        {
            var tech = technicians[8];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Brake System Repair", "Expert", -15, verifier?.UserId),
                CreateSkill(tech.UserId, "Suspension System", "Intermediate", -14, verifier?.UserId),
                CreateSkill(tech.UserId, "Tire Services", "Intermediate", -12, verifier?.UserId),
                CreateSkill(tech.UserId, "Battery Replacement", "Beginner", -10, null)
            });
        }

        // Technician 10: Junior (Service Center 2)
        if (technicians.Count > 9)
        {
            var tech = technicians[9];
            skills.AddRange(new[]
            {
                CreateSkill(tech.UserId, "Tire Services", "Intermediate", -12, verifier?.UserId),
                CreateSkill(tech.UserId, "Diagnostics", "Beginner", -10, null),
                CreateSkill(tech.UserId, "Brake System Repair", "Beginner", -8, null)
            });
        }

        context.EmployeeSkills.AddRange(skills);
        context.SaveChanges();

        Console.WriteLine($"✅ Seeded {skills.Count} employee skills for {technicians.Count} technicians");
    }

    /// <summary>
    /// Create a skill with realistic certification data
    /// </summary>
    /// <param name="userId">Technician user ID</param>
    /// <param name="skillName">Skill name</param>
    /// <param name="skillLevel">Beginner, Intermediate, or Expert</param>
    /// <param name="monthsAgo">How many months ago was certification obtained (negative number)</param>
    /// <param name="verifiedBy">UserID of verifier (null if not verified)</param>
    private static EmployeeSkill CreateSkill(
        int userId,
        string skillName,
        string skillLevel,
        int monthsAgo,
        int? verifiedBy)
    {
        var certDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(monthsAgo));
        var expiryDate = certDate.AddYears(3); // 3-year validity

        return new EmployeeSkill
        {
            UserId = userId,
            SkillName = skillName,
            SkillLevel = skillLevel,
            CertificationDate = certDate,
            ExpiryDate = expiryDate,
            CertifyingBody = "EV Technician Association",
            CertificationNumber = $"CERT-{userId:D4}-{skillName.Replace(" ", "").ToUpper()}-{certDate:yyyyMM}",
            IsVerified = verifiedBy.HasValue,
            VerifiedBy = verifiedBy,
            VerifiedDate = verifiedBy.HasValue ? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(monthsAgo + 1)) : null,
            Notes = skillLevel == "Expert" ? "Advanced certification with practical assessment" : null
        };
    }
}
