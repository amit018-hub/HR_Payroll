using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.Payroll
{
    public class PayrollRunRowDto
    {
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? DepartmentName { get; set; }
        public bool HasSalaryComponents { get; set; }

        // Null = not yet calculated; populated after CalculatePayroll
        public decimal? Gross { get; set; }
        public decimal? Deductions { get; set; }
        public decimal? NetPay { get; set; }

        /// <summary>Draft | Calculated | Approved | Paid</summary>
        public string Status { get; set; } = "Not Calculated";

        // PayrollEmployee PK for update operations
        public int? PayrollEmployeeId { get; set; }
    }
}
