using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.Payroll
{
    public class SalarySlipDto
    {
        public int EmployeeID { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? Designation { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? MaskedBankAccount { get; set; }

        public int PayrollMonth { get; set; }
        public int PayrollYear { get; set; }

        public List<PayrollComponentLineDto> Earnings { get; set; } = new();
        public List<PayrollComponentLineDto> Deductions { get; set; } = new();

        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public string NetSalaryInWords { get; set; } = string.Empty;

        /// <summary>Draft | Calculated | Approved | Paid | Not Calculated</summary>
        public string Status { get; set; } = "Not Calculated";
    }
}
