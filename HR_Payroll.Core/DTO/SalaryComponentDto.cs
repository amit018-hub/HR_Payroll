using System.Collections.Generic;

namespace HR_Payroll.Core.Models
{
    public class SalaryComponentDto
    {
        public int? ComponentId { get; set; }
        public string ComponentName { get; set; }
        public decimal Amount { get; set; }
    }

    public class EmployeeSalaryBreakupViewModel
    {
        public int? EmployeeId { get; set; }
        public List<SalaryComponentDto> SalaryComponents { get; set; } = new();
    }
}