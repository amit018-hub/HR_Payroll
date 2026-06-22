using HR_Payroll.Core.DTO.Payroll;
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
        Task<EmployeeSalary?> SaveEmployeeSalaryMasterAsync(EmployeeSalary master);
        Task<bool> SaveEmployeeSalaryComponentsAsync(int employeeId, List<EmployeePayrollSalaryComponent> components, int payrollMonth, int payrollYear);

        Task<List<PayrollRunRowDto>> GetPayrollRunRowsAsync(string payrollMonth, int? departmentId);
        Task<PayrollRunResultDto> CalculatePayrollAsync(string payrollMonth, List<int> employeeIds, int createdByUserId);

        // Salary Slip — reads from Payroll table + EmployeeSalaryComponent
        Task<SalarySlipDto?> GetSalarySlipAsync(int employeeId, int payrollMonth, int payrollYear);

        // Bank Payment — uses PayrollRun + PayrollEmployee + EmployeeBank
        Task<BankPaymentSummaryDto> GetBankPaymentSummaryAsync(string payrollMonth);
        Task<bool> MarkPaymentDoneAsync(string payrollMonth, List<int> employeeIds, int modifiedByUserId);
    }
}
