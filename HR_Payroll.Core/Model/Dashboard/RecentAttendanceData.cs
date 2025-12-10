using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.Dashboard
{
    public class RecentAttendanceData
    {
        public string? EmployeeName { get; set; }
        public string? Date { get; set; }
        public string? CheckIn { get; set; }
        public string? CheckOut { get; set; }
        public string? Status { get; set; }
    }
}
