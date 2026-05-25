using Dapper;
using HR_Payroll.Core.DTO.TimeSheet;
using HR_Payroll.Core.Model.TimeSheet;
using HR_Payroll.Core.Services;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;

public class TimeSheetServices : ITimeSheetServices
{
    private readonly string _connStr;
    private readonly ILogger<TimeSheetServices> _logger;

    public TimeSheetServices(IConfiguration config, ILogger<TimeSheetServices> logger)
    {
        _logger = logger;
        _connStr = config.GetConnectionString("DefaultConnection")
                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private SqlConnection CreateConnection() => new SqlConnection(_connStr);

    // ── Week calculation: same Monday-anchor logic as the JS ─
    // JS getCurrentMonday() → shifts to Monday of current week.
    // C# equivalent: find Monday of today, then add offset weeks.
    private static DateTime ComputeWeekStart(int weekOffset)
    {
        var today = DateTime.Today;
        int dow = (int)today.DayOfWeek;                // Sun=0 … Sat=6
        int diff = (dow == 0) ? -6 : 1 - dow;          // shift so Mon=0
        var monday = today.AddDays(diff);                 // Monday of current week
        return monday.AddDays(weekOffset * 7);            // apply offset
    }

    // ════════════════════════════════════════════════════════
    //  GET TIMESHEET
    //  Calls sp_GetTimesheetByWeek → 2 result-sets (header + rows)
    //  Calls sp_GetTimesheetDropdowns → 4 result-sets
    //  Calls sp_GetPublicHolidays → 1 result-set
    // ════════════════════════════════════════════════════════
    public async Task<Result<TimesheetViewModel>> GetViewModelAsync(
        string employeeCode, int weekOffset)
    {
        try
        {
            var weekStart = ComputeWeekStart(weekOffset);

            using var db = CreateConnection();

            // ── 1. Dropdowns (4 result-sets in one round-trip) ──
            using var multiDrop = await db.QueryMultipleAsync(
                "sp_GetTimesheetDropdowns",
                commandType: CommandType.StoredProcedure);

            var projects = (await multiDrop.ReadAsync<DropdownItem>()).ToList();
            var activities = (await multiDrop.ReadAsync<DropdownItem>()).ToList();
            var categories = (await multiDrop.ReadAsync<DropdownItem>()).ToList();
            var shifts = (await multiDrop.ReadAsync<DropdownItem>()).ToList();

            // ── 2. Holidays ──────────────────────────────────────
            var holidays = (await db.QueryAsync<PublicHolidayDto>(
                "sp_GetPublicHolidays",
                new { Year = weekStart.Year },
                commandType: CommandType.StoredProcedure))
                .Select(h => h.HolidayDate.ToString("yyyy-MM-dd"))
                .ToList();

            // ── 3. Header + Rows (2 result-sets in one round-trip) ─
            using var multiTs = await db.QueryMultipleAsync(
                "sp_GetTimesheetByWeek",
                new { EmployeeCode = employeeCode, WeekStart = weekStart },
                commandType: CommandType.StoredProcedure);

            var header = await multiTs.ReadFirstOrDefaultAsync<TimesheetHeaderDto>();
            var rows = (await multiTs.ReadAsync<TimesheetRowDto>()).ToList();

            var vm = new TimesheetViewModel
            {
                Header = header ?? new TimesheetHeaderDto(),
                Rows = rows,
                Projects = projects,
                Activities = activities,
                Categories = categories,
                Shifts = shifts,
                Holidays = holidays,
                WeekOffset = weekOffset
            };

            return Result<TimesheetViewModel>.Success(vm, "Timesheet retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetViewModelAsync failed for employee {Code} offset {Offset}",
                employeeCode, weekOffset);
            return Result<TimesheetViewModel>.Failure("Failed to retrieve timesheet.");
        }
    }

    // ════════════════════════════════════════════════════════
    //  SAVE
    //  JS sends weekOffset (int) – we compute weekStart here.
    //  ShiftId is a string (ShiftMaster.ShiftCode).
    // ════════════════════════════════════════════════════════
    public async Task<Result<object>> SaveAsync(SaveTimesheetRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeCode))
                return Result<object>.Failure("Employee code is required.");

            // Compute weekStart from the offset the JS sent
            var weekStart = ComputeWeekStart(request.WeekOffset);

            // Serialize rows to the JSON format sp_SaveTimesheet expects
            var rowsJson = JsonSerializer.Serialize(
                request.Rows.Select((r, i) => new
                {
                    rowId = r.RowId,
                    projectId = r.ProjectId,
                    activityId = r.ActivityId,
                    categoryId = r.CategoryId,
                    shiftId = r.ShiftId,          // string – ShiftCode
                    remarks = r.Remarks ?? string.Empty,
                    sortOrder = i + 1,
                    hours = r.Hours             // decimal?[7]
                }),
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            using var db = CreateConnection();

            var result = await db.QueryFirstAsync(
                "sp_SaveTimesheet",
                new
                {
                    EmployeeCode = request.EmployeeCode,
                    WeekStart = weekStart,
                    RowsJson = rowsJson
                },
                commandType: CommandType.StoredProcedure);

            // SP returns: TimesheetId INT, Status VARCHAR
            int savedId = (int)(result.TimesheetId ?? 0);

            return Result<object>.Success(
                new { timesheetId = savedId },
                "Timesheet saved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveAsync failed for employee {Code}", request.EmployeeCode);
            return Result<object>.Failure("Save failed.");
        }
    }

    // ════════════════════════════════════════════════════════
    //  SUBMIT
    //  SP returns: Success BIT, Message NVARCHAR
    // ════════════════════════════════════════════════════════
    public async Task<Result<object>> SubmitAsync(SubmitTimesheetRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeCode))
                return Result<object>.Failure("Employee code is required.");

            using var db = CreateConnection();

            var r = await db.QueryFirstAsync(
                "sp_SubmitTimesheet",
                new
                {
                    TimesheetId = request.TimesheetId,
                    EmployeeCode = request.EmployeeCode
                },
                commandType: CommandType.StoredProcedure);

            bool success = (bool)(r.Success ?? false);
            string msg = (string)(r.Message ?? string.Empty);

            return success
                ? Result<object>.Success(null, msg)
                : Result<object>.Failure(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubmitAsync failed for TimesheetId {Id}", request.TimesheetId);
            return Result<object>.Failure("Submit failed.");
        }
    }

    // ════════════════════════════════════════════════════════
    //  DELETE ROW
    //  SP returns: Success BIT, Message NVARCHAR
    // ════════════════════════════════════════════════════════
    public async Task<Result<object>> DeleteRowAsync(int rowId, string employeeCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
                return Result<object>.Failure("Employee code is required.");

            using var db = CreateConnection();

            var r = await db.QueryFirstAsync(
                "sp_DeleteTimesheetRow",
                new { RowId = rowId, EmployeeCode = employeeCode },
                commandType: CommandType.StoredProcedure);

            bool success = (bool)(r.Success ?? false);
            string msg = (string)(r.Message ?? string.Empty);

            return success
                ? Result<object>.Success(null, msg)
                : Result<object>.Failure(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteRowAsync failed for RowId {Id}", rowId);
            return Result<object>.Failure("Delete failed.");
        }
    }

    // ════════════════════════════════════════════════════════
    //  RESET ROW
    //  SP returns: Success BIT, Message NVARCHAR
    // ════════════════════════════════════════════════════════
    public async Task<Result<object>> ResetRowAsync(int rowId, string employeeCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
                return Result<object>.Failure("Employee code is required.");

            using var db = CreateConnection();

            var r = await db.QueryFirstAsync(
                "sp_ResetTimesheetRow",
                new { RowId = rowId, EmployeeCode = employeeCode },
                commandType: CommandType.StoredProcedure);

            bool success = (bool)(r.Success ?? false);
            string msg = (string)(r.Message ?? string.Empty);

            return success
                ? Result<object>.Success(null, msg)
                : Result<object>.Failure(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResetRowAsync failed for RowId {Id}", rowId);
            return Result<object>.Failure("Reset failed.");
        }
    }

    // ════════════════════════════════════════════════════════
    //  GET HISTORY
    //  SP returns: LogId, ActionType, ActionOn, Remarks,
    //              ActionByCode, ActionByName
    // ════════════════════════════════════════════════════════
    public async Task<Result<IEnumerable<TimesheetAuditDto>>> GetHistoryAsync(int timesheetId)
    {
        try
        {
            using var db = CreateConnection();

            var data = await db.QueryAsync<TimesheetAuditDto>(
                "sp_GetTimesheetHistory",
                new { TimesheetId = timesheetId },
                commandType: CommandType.StoredProcedure);

            return Result<IEnumerable<TimesheetAuditDto>>
                .Success(data, "History retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetHistoryAsync failed for TimesheetId {Id}", timesheetId);
            return Result<IEnumerable<TimesheetAuditDto>>.Failure("Failed to retrieve history.");
        }
    }
}