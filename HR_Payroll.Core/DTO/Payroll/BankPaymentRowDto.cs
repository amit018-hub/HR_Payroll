using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.Payroll
{
    public class BankPaymentRowDto
    {
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? BankName { get; set; }
        public string? AccountNo { get; set; }
        public string? IFSC { get; set; }
        public decimal NetPay { get; set; }
        public string Status { get; set; } = "Calculated";
        public bool HasBankDetails { get; set; }

        // PayrollEmployee PK — needed for MarkPaymentDone
        public int PayrollEmployeeId { get; set; }
    }
}
