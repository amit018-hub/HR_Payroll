using HR_Payroll.Core.DTO.TimeSheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.TimeSheet
{
    public class TimesheetViewModel
    {
        public TimesheetHeaderDto Header { get; set; } = new();
        public List<TimesheetRowDto> Rows { get; set; } = new();
        public List<DropdownItem> Projects { get; set; } = new();
        public List<DropdownItem> Activities { get; set; } = new();
        public List<DropdownItem> Categories { get; set; } = new();
        public List<DropdownItem> Shifts { get; set; } = new();
        public List<string> Holidays { get; set; } = new();  // "yyyy-MM-dd"
        public int WeekOffset { get; set; }
    }
}
