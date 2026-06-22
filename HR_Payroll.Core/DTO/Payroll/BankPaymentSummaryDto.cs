using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.Payroll
{
    public class BankPaymentSummaryDto
    {
        public string? PayrollMonth { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalAmount { get; set; }
        public List<BankPaymentRowDto> Rows { get; set; } = new();
    }
}
