using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.Payroll
{
    public class PayrollRunResultDto
    {
        public int PayrollRunId { get; set; }
        public string? PayrollMonth { get; set; }
        public string? Status { get; set; }
        public List<PayrollRunRowDto> Rows { get; set; } = new();
    }
}
