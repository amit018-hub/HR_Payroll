using HR_Payroll.Core.DTO;
using System;
using System.Collections.Generic;

namespace HR_Payroll.Core.DTO
{
    public class EmployeeDetailsModel
    {
        public EmployeeBasicDto Basic { get; set; } = new();
        public EmployeeBankDto? Bank { get; set; }
        public PayrollDto? Payroll { get; set; }
        public List<SalaryComponentDto> Components { get; set; } = new();
    }

    

  

  


}