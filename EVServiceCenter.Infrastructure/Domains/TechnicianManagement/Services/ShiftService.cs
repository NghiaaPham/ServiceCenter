using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.TechnicianManagement.Services;

public class ShiftService : IShiftService
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ShiftService> _logger;
    private readonly EVDbContext _context;

    // ✅ Cache timezone VN, có fallback cho Linux (Asia/Ho_Chi_Minh)
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        try
        {
            // Windows
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            // Linux / container
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
    }

    public ShiftService(
        IShiftRepository shiftRepository,
        IConfiguration configuration,
        ILogger<ShiftService> logger,
        EVDbContext context)
    {
        _shiftRepository = shiftRepository;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    public async Task<ShiftResponseDto> CheckInAsync(
        int technicianId,
        CheckInRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // ✅ Luôn lưu DB bằng UTC, nhưng tính “hôm nay” & đi trễ theo giờ VN
        var utcNow = DateTime.UtcNow;
        var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, VietnamTimeZone);
        var today = DateOnly.FromDateTime(vietnamNow);

        _logger.LogInformation(
            "Processing check-in for Technician {TechnicianId} at {TimeUtc} (VN: {TimeVn})",
            technicianId, utcNow, vietnamNow);

        // 1. Check if already checked in today
        var existingShift = await _shiftRepository
            .GetByTechnicianAndDateAsync(technicianId, today, cancellationToken);

        if (existingShift != null &&
            existingShift.CheckInTime.HasValue &&
            !existingShift.CheckOutTime.HasValue)
        {
            var existedCheckInVn = TimeZoneInfo.ConvertTimeFromUtc(
                existingShift.CheckInTime.Value,
                VietnamTimeZone);

            var checkInTimeFormatted = existedCheckInVn.ToString("HH:mm");
            throw new InvalidOperationException(
                $"Already checked in at {checkInTimeFormatted}. Please check-out first before checking in again.");
        }

        // 2. LOAD SERVICE CENTER to get working hours
        var serviceCenter = existingShift?.Center
            ?? await _context.ServiceCenters
                .FirstOrDefaultAsync(sc => sc.CenterId == request.ServiceCenterId, cancellationToken);

        if (serviceCenter == null)
        {
            throw new InvalidOperationException(
                $"Service Center ID {request.ServiceCenterId} not found");
        }

        // 3. AUTO-DETECT ShiftType based on check-in time (VN) and center's working hours
        var checkInTimeOnly = TimeOnly.FromDateTime(vietnamNow);
        var (shiftType, startTime, endTime) = DetermineShiftTimes(
            checkInTimeOnly,
            serviceCenter.OpenTime,
            serviceCenter.CloseTime,
            request.ShiftType);

        _logger.LogInformation(
            "Check-in at {CheckInTime} (VN), Center hours: {OpenTime}-{CloseTime}, " +
            "Detected shift: {ShiftType} ({Start}-{End})",
            checkInTimeOnly, serviceCenter.OpenTime, serviceCenter.CloseTime,
            shiftType, startTime, endTime);

        var gracePeriodMinutes = _configuration.GetValue<int>("AttendanceSettings:GracePeriodMinutes", 15);

        // 4. Create or update shift (CheckInTime lưu UTC)
        Shift shift;
        if (existingShift == null)
        {
            shift = new Shift
            {
                UserId = technicianId,
                CenterId = request.ServiceCenterId,
                ShiftDate = today,
                StartTime = startTime,
                EndTime = endTime,
                ShiftType = shiftType,
                Status = "Present",
                CheckInTime = utcNow,
                Notes = request.Notes,
                CreatedDate = utcNow,
                CreatedBy = technicianId
            };

            shift = await _shiftRepository.CreateAsync(shift, cancellationToken);
        }
        else
        {
            shift = existingShift;
            shift.CheckInTime = utcNow;
            shift.CheckOutTime = null;  // ✅ CLEAR checkout time cũ
            shift.ShiftType = shiftType;
            shift.StartTime = startTime;
            shift.EndTime = endTime;
            shift.Status = "Present";

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                shift.Notes = request.Notes;
            }

            shift = await _shiftRepository.UpdateAsync(shift, cancellationToken);
        }

        // 5. Calculate IsLate (so sánh theo VN time)
        var lateThreshold = startTime.AddMinutes(gracePeriodMinutes);
        shift.IsLate = checkInTimeOnly > lateThreshold;

        var lateMinutes = shift.IsLate == true
            ? (int)(checkInTimeOnly.ToTimeSpan() - startTime.ToTimeSpan()).TotalMinutes
            : 0;

        if (shift.IsLate == true)
        {
            _logger.LogWarning(
                "Technician {TechnicianId} checked in late: {CheckInTime} (Scheduled: {ScheduledTime}, Late by: {LateMinutes} minutes)",
                technicianId, checkInTimeOnly, startTime, lateMinutes);
        }

        await _shiftRepository.UpdateAsync(shift, cancellationToken);

        _logger.LogInformation(
            "Technician {TechnicianId} checked in successfully. ShiftId={ShiftId}, IsLate={IsLate}",
            technicianId, shift.ShiftId, shift.IsLate);

        // DTO tự tính LateMinutes, nên không cần truyền
        return MapToResponseDto(shift);
    }

    public async Task<ShiftResponseDto> CheckOutAsync(
        int technicianId,
        CheckOutRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, VietnamTimeZone);
        var today = DateOnly.FromDateTime(vietnamNow);

        _logger.LogInformation(
            "Processing check-out for Technician {TechnicianId} at {TimeUtc} (VN: {TimeVn})",
            technicianId, utcNow, vietnamNow);

        // 1. Get shift for today
        var shift = await _shiftRepository
            .GetByTechnicianAndDateAsync(technicianId, today, cancellationToken);

        if (shift == null)
        {
            throw new InvalidOperationException("No shift found for today. Please check-in first.");
        }

        if (!shift.CheckInTime.HasValue)
        {
            throw new InvalidOperationException("Must check-in first before checking out.");
        }

        if (shift.CheckOutTime.HasValue)
        {
            throw new InvalidOperationException("Already checked out.");
        }

        // 2. Set check-out time (UTC)
        shift.CheckOutTime = utcNow;

        // 3. Calculate worked hours
        var totalMinutes = (utcNow - shift.CheckInTime.Value).TotalMinutes;
        shift.WorkedHours = (decimal)(totalMinutes / 60.0);

        // 4. Calculate net working hours (subtract break)
        var breakMinutes = _configuration.GetValue<int>("AttendanceSettings:BreakDurationMinutes", 60);
        var netMinutes = Math.Max(0, totalMinutes - breakMinutes);
        shift.NetWorkingHours = (decimal)(netMinutes / 60.0);

        // 5. Check if early leave (so sánh theo VN time)
        var checkOutTimeOnly = TimeOnly.FromDateTime(vietnamNow);
        var gracePeriodMinutes = _configuration.GetValue<int>("AttendanceSettings:GracePeriodMinutes", 15);
        var earlyThreshold = shift.EndTime.AddMinutes(-gracePeriodMinutes);
        shift.IsEarlyLeave = checkOutTimeOnly < earlyThreshold;

        if (shift.IsEarlyLeave == true)
        {
            _logger.LogWarning(
                "Technician {TechnicianId} checked out early: {CheckOutTime} (Scheduled: {ScheduledEndTime})",
                technicianId, checkOutTimeOnly, shift.EndTime);
        }

        // 6. Update status
        shift.Status = "Completed";

        await _shiftRepository.UpdateAsync(shift, cancellationToken);

        _logger.LogInformation(
            "Technician {TechnicianId} checked out successfully. WorkedHours={WorkedHours}, NetWorkingHours={NetWorkingHours}",
            technicianId, shift.WorkedHours, shift.NetWorkingHours);

        return MapToResponseDto(shift);
    }

    public async Task<bool> IsOnShiftAsync(
        int technicianId,
        DateTime? dateTime = null,
        CancellationToken cancellationToken = default)
    {
        var currentUtc = dateTime ?? DateTime.UtcNow;
        var currentVn = TimeZoneInfo.ConvertTimeFromUtc(currentUtc, VietnamTimeZone);
        var today = DateOnly.FromDateTime(currentVn);

        var shift = await _shiftRepository
            .GetByTechnicianAndDateAsync(technicianId, today, cancellationToken);

        if (shift == null)
        {
            _logger.LogDebug("No shift found for Technician {TechnicianId} on {Date}", technicianId, today);
            return false;
        }

        if (!shift.CheckInTime.HasValue)
        {
            _logger.LogDebug("Technician {TechnicianId} has not checked in for shift {ShiftId}", technicianId, shift.ShiftId);
            return false;
        }

        if (shift.CheckOutTime.HasValue)
        {
            _logger.LogDebug("Technician {TechnicianId} has already checked out from shift {ShiftId}", technicianId, shift.ShiftId);
            return false;
        }

        _logger.LogDebug(
            "Technician {TechnicianId} is on-shift. ShiftId={ShiftId}, CheckInTimeUtc={CheckInTime}",
            technicianId, shift.ShiftId, shift.CheckInTime);

        return true;
    }

    public async Task<ShiftResponseDto?> GetTodayShiftAsync(
        int technicianId,
        CancellationToken cancellationToken = default)
    {
        var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
        var today = DateOnly.FromDateTime(vietnamNow);

        var shift = await _shiftRepository
            .GetByTechnicianAndDateAsync(technicianId, today, cancellationToken);

        if (shift == null)
        {
            return null;
        }

        return MapToResponseDto(shift);
    }

    public async Task<List<ShiftResponseDto>> GetShiftHistoryAsync(
        int technicianId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var shifts = await _shiftRepository
            .GetShiftsByDateRangeAsync(technicianId, from, to, cancellationToken);

        var result = new List<ShiftResponseDto>();
        foreach (var shift in shifts)
        {
            result.Add(MapToResponseDto(shift));
        }

        return result;
    }

    private ShiftResponseDto MapToResponseDto(Shift shift)
    {
        // ✅ Convert UTC to Vietnam time for display
        DateTime? checkInVn = shift.CheckInTime.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(shift.CheckInTime.Value, VietnamTimeZone)
            : null;

        DateTime? checkOutVn = shift.CheckOutTime.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(shift.CheckOutTime.Value, VietnamTimeZone)
            : null;

        return new ShiftResponseDto
        {
            ShiftId = shift.ShiftId,
            TechnicianId = shift.UserId,
            TechnicianName = shift.User?.FullName ?? string.Empty,
            ServiceCenterId = shift.CenterId,
            ServiceCenterName = shift.Center?.CenterName ?? string.Empty,
            ShiftDate = shift.ShiftDate,
            ShiftType = shift.ShiftType ?? "FullDay",
            CheckInTime = checkInVn,
            CheckOutTime = checkOutVn,
            WorkedHours = shift.WorkedHours,
            NetWorkingHours = shift.NetWorkingHours,
            IsLate = shift.IsLate ?? false,
            IsEarlyLeave = shift.IsEarlyLeave ?? false,
            Status = shift.Status ?? "Unknown",
            Notes = shift.Notes,
            ScheduledStartTime = shift.StartTime,
            ScheduledEndTime = shift.EndTime
            // LateMinutes: computed trong DTO dựa trên CheckInTime + ScheduledStartTime
        };
    }

    /// <summary>
    /// ✅ SMART SHIFT DETECTION based on Service Center working hours
    /// 
    /// Algorithm:
    /// 1. Nếu user override ShiftType → validate & dùng nó (FullDay = full giờ center)
    /// 2. Nếu không → auto-detect dựa vào:
    ///    - Center OpenTime/CloseTime
    ///    - Check-in time hiện tại (VN)
    ///    - Chia làm 3 ca: Morning, Afternoon, Evening
    /// </summary>
    private (string ShiftType, TimeOnly StartTime, TimeOnly EndTime) DetermineShiftTimes(
        TimeOnly checkInTime,
        TimeOnly centerOpenTime,
        TimeOnly centerCloseTime,
        string? requestedShiftType)
    {
        // 1. User override - validate hợp lệ
        if (!string.IsNullOrWhiteSpace(requestedShiftType))
        {
            var validTypes = new[] { "Morning", "Afternoon", "Evening", "Night", "FullDay" };
            if (validTypes.Contains(requestedShiftType.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                if (requestedShiftType.Equals("FullDay", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("User requested FullDay shift");
                    return ("FullDay", centerOpenTime, centerCloseTime);
                }

                _logger.LogInformation("User requested shift type: {Type}", requestedShiftType);
                // Với Morning/Afternoon/Evening/Night → dùng label này ở dưới
            }
        }

        // 2. Tính total working hours của center
        var open = centerOpenTime;
        var close = centerCloseTime;
        var totalMinutes = (int)(close.ToTimeSpan() - open.ToTimeSpan()).TotalMinutes;

        if (totalMinutes <= 0)
        {
            // Guard cho config sai (Close <= Open)
            _logger.LogError(
                "Invalid center working hours: {Open}-{Close}. Falling back to 8h shift.",
                centerOpenTime, centerCloseTime);

            totalMinutes = 8 * 60;
            close = open.AddHours(8);
        }

        var totalHours = totalMinutes / 60.0;

        _logger.LogDebug(
            "Center working hours: {Open}-{Close} = {Hours}h",
            open, close, totalHours);

        // 3. AUTO-DETECT ca dựa vào check-in time
        // Case 1: Check-in trước giờ mở → Night/Early
        if (checkInTime < open)
        {
            _logger.LogWarning(
                "Check-in {Time} is BEFORE center opens {OpenTime}",
                checkInTime, open);
            return ("Night", new TimeOnly(0, 0), open);
        }

        // Case 2: Check-in sau giờ đóng → Night/Late
        if (checkInTime >= close)
        {
            _logger.LogWarning(
                "Check-in {Time} is AFTER center closes {CloseTime}",
                checkInTime, close);
            return ("Night", close, new TimeOnly(23, 59));
        }

        // Case 3: Chia ca theo % thời gian làm việc
        var minutesFromOpen = (int)(checkInTime.ToTimeSpan() - open.ToTimeSpan()).TotalMinutes;
        var percentOfDay = (double)minutesFromOpen / totalMinutes;

        TimeOnly shiftStart;
        TimeOnly shiftEnd;
        string detectedType;

        if (percentOfDay < 0.35) // First 35% → Morning
        {
            detectedType = "Morning";
            shiftStart = open;
            shiftEnd = open.AddMinutes((int)(totalMinutes * 0.35));
        }
        else if (percentOfDay < 0.70) // 35–70% → Afternoon
        {
            detectedType = "Afternoon";
            shiftStart = open.AddMinutes((int)(totalMinutes * 0.35));
            shiftEnd = open.AddMinutes((int)(totalMinutes * 0.70));
        }
        else // Last 30% → Evening
        {
            detectedType = "Evening";
            shiftStart = open.AddMinutes((int)(totalMinutes * 0.70));
            shiftEnd = close;
        }

        // 4. Override type nếu user request hợp lệ (trừ FullDay đã xử lý ở trên)
        if (!string.IsNullOrWhiteSpace(requestedShiftType) &&
            !requestedShiftType.Equals("FullDay", StringComparison.OrdinalIgnoreCase))
        {
            detectedType = requestedShiftType.Trim();
        }

        _logger.LogInformation(
            "Shift detected: {Type} ({Start}-{End}), Check-in at {Percent:P0} of working day",
            detectedType, shiftStart, shiftEnd, percentOfDay);

        return (detectedType, shiftStart, shiftEnd);
    }
}

