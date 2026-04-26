using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.TimeSheet
{
    public class TimesheetHeaderDto
    {
        public int TimesheetId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;   // FirstName + ' ' + LastName
        public string ApproverCode { get; set; } = string.Empty;
        public string ApproverName { get; set; } = string.Empty;
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public string StatusCode { get; set; } = "DRAFT";

        // Convenience alias consumed by the Razor view JS
        public string WeekStart => WeekStartDate.ToString("yyyy-MM-dd");
        public string Status => StatusCode;
    }
}
