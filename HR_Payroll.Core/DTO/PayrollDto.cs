using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO
{
    public class PayrollDto
    {
        public decimal? Amount { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public int? PayrollMonth { get; set; }
        public int? PayrollYear { get; set; }
    }
}
