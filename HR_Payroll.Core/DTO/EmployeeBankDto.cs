using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO
{
    public class EmployeeBankDto
    {
        public string? BeneficiaryName { get; set; }
        public string? BankName { get; set; }
        public string? AccountNo { get; set; }
        public string? IFSC { get; set; }
    }
}
