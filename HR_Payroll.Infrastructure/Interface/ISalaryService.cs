using HR_Payroll.Core.Entity;
using HR_Payroll.Infrastructure.Concrete;
using HR_Payroll.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Interface
{
    public interface ISalaryService
    {
        Task<List<SalaryComponents>> GetAllSalaryComponentsAsync();
        Task<EmployeeSalaryMaster?> SaveEmployeeSalaryMasterAsync(EmployeeSalaryMaster master);
        Task<bool> SaveEmployeeSalaryComponentsAsync(int employeeId, List<EmployeePayrollSalaryComponent> components, int payrollMonth, int payrollYear);
    }
}
