using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.TimeSheet
{
    public class SaveTimesheetRequest
    {
        public int TimesheetId { get; set; }
        public int WeekOffset { get; set; }   // JS sends offset, NOT a date string
        public string EmployeeCode { get; set; } = string.Empty;  // injected by controller
        public List<RowSaveItem> Rows { get; set; } = new();
    }
}
