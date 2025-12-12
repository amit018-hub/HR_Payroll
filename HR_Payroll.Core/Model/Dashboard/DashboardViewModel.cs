using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.Dashboard
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int PresentToday { get; set; }
        public int AbsentToday { get; set; }
        public int LeaveToday { get; set; }
        public int WFHCount { get; set; }
        public decimal TodayPresentPercentage { get; set; }
        public decimal LeavePercentage { get; set; }
        public decimal WFHPercentage { get; set; }
        public List<AttendanceChartData> AttendanceChart { get; set; } = new List<AttendanceChartData>();
        public List<LeaveTypeSummary> LeaveTypeSummary { get; set; } = new List<LeaveTypeSummary>();
        public List<RecentAttendanceData> RecentAttendance { get; set; } = new List<RecentAttendanceData>();
    }
}
