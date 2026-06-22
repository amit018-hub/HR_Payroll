using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.Payroll
{
    public class MarkPaymentDoneRequest
    {
        /// <summary>"yyyy-MM"</summary>
        public string? PayrollMonth { get; set; }
        public List<int> PayrollEmployeeIds { get; set; } = new();
    }
}
