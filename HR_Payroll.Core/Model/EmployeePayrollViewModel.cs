using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model
{
    public class EmployeePayrollViewModel
    {
        public int? EmployeeId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SalaryPerMonth { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SalaryPerYear { get; set; }

        public string? RecoveryMode { get; set; }
        public decimal? InstallmentAmount { get; set; }
        public string? RecoveryCycle { get; set; }
        public string? BiometricUserId { get; set; }
    }
}
