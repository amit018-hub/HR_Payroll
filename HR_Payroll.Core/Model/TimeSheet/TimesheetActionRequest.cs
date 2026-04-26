using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.TimeSheet
{
    public class TimesheetActionRequest
    {
        public int TimesheetId { get; set; }
        public int RowId { get; set; }
        public string EmployeeCode { get; set; }
    }
}
