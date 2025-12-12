using HR_Payroll.Core.Entity;
using HR_Payroll.Core.Model;
using HR_Payroll.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Interface
{
    public interface IEmployeeService
    {
        Task<Employees?> SaveBasicInfoAsync(EmployeeBasicInfoViewModel model, string? profilePicPath = null);
        Task<EmployeeBank?> SaveBankDetailsAsync(EmployeeBankViewModel model);
        Task<int> SaveAllEmployeeDataAsync( EmployeeBasicInfoViewModel basic, EmployeePayrollViewModel payroll, EmployeeBankViewModel bank, List<SalaryComponentViewModel>? components, string? profileImage);
    }
}
