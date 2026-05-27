using HR_Payroll.Core.DTO.TimeSheet;
using HR_Payroll.Core.Model.TimeSheet;
using HR_Payroll.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Interface
{
    public interface ITimeSheetServices
    {
        Task<Result<TimesheetViewModel>> GetViewModelAsync(string employeeCode, int weekOffset);

        Task<Result<object>> SaveAsync(SaveTimesheetRequest request);

        Task<Result<object>> SubmitAsync(SubmitTimesheetRequest request);

        Task<Result<object>> DeleteRowAsync(int rowId, string employeeCode);

        Task<Result<object>> ResetRowAsync(int rowId, string employeeCode);

        Task<Result<IEnumerable<TimesheetAuditDto>>> GetHistoryAsync(int timesheetId);

        Task<Result<TeamTimesheetResponse>> GetTeamTimesheetsAsync(string approverCode, int weekOffset);

        Task<Result<TeamTimesheetDetail>> GetTimesheetByIdAsync(int timesheetId, int employeeId, string approverCode);

        Task<Result<object>> ApproveRejectAsync(ApproveRejectRequest request);
    }
}
