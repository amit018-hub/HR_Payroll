using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.Dashboard
{
    public class EmployeeDashboardViewModel
    {
        public string? TodayStatus { get; set; }
        public string? TodayWorkingHours { get; set; }
        public string? MonthlyWorkingHours { get; set; }
        public int LeaveTaken { get; set; }

        public List<EmployeeAttendanceChartData> AttendanceChart { get; set; }
        public List<EmployeeRecentAttendanceData> RecentAttendance { get; set; }
    }
}
