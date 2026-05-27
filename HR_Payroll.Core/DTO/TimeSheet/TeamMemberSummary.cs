using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.TimeSheet
{
    public class TeamMemberSummary
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public int TimesheetId { get; set; }
        public string WeekStartDate { get; set; } = string.Empty;  // "yyyy-MM-dd"
        public string WeekEndDate { get; set; } = string.Empty;
        public string StatusCode { get; set; } = "DRAFT";
        public DateTime? SubmittedOn { get; set; }
        public string? RejectionNote { get; set; }
        public decimal TotalHours { get; set; }
        public int PresentDays { get; set; }
        public int TotalRows { get; set; }
    }
}
