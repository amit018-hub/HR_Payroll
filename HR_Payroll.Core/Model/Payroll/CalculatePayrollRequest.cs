using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.Payroll
{
    public class CalculatePayrollRequest
    {
        /// <summary>"yyyy-MM" from input[type=month]</summary>
        public string? Month { get; set; }
        public List<int> EmployeeIds { get; set; } = new();
    }
}
