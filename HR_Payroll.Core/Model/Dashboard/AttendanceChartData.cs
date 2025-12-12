using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.Dashboard
{
    public class AttendanceChartData
    {
        public string? Date { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int WFHCount { get; set; }
        public int TotalCount => PresentCount + AbsentCount + WFHCount;
    }
}
