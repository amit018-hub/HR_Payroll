using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.Payroll
{
    public class PayrollComponentLineDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
