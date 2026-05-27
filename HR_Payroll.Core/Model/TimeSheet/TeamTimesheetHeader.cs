using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.TimeSheet
{
    public class TeamTimesheetHeader
    {
        public int TimesheetId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string ApproverCode { get; set; } = string.Empty;
        public string ApproverName { get; set; } = string.Empty;
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public string StatusCode { get; set; } = "DRAFT";
        public DateTime? SubmittedOn { get; set; }
        public string? RejectionNote { get; set; }
    }
}
