using HR_Payroll.Core.Entity;
using HR_Payroll.Infrastructure.Data;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Concrete
{
    public class SalaryService: ISalaryService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<SalaryService> _logger;

        public SalaryService(AppDbContext context, ILogger<SalaryService> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<SalaryComponents>> GetAllSalaryComponentsAsync()
        {
            try
            {
                return await _context.SalaryComponents
                    .Where(c => c.Del_Flg != "Y")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching salary components");
                return new List<SalaryComponents>();
            }
        }

        public async Task<EmployeeSalaryMaster?> SaveEmployeeSalaryMasterAsync(EmployeeSalaryMaster master)
        {
            try
            {
                if (master == null) return null;

                // expire existing active master (if any)
                var existing = await _context.EmployeeSalaryMasters
                    .Where(s => s.EmployeeID == master.EmployeeID && (s.IsActive == 1 || s.IsActive == null))
                    .OrderByDescending(s => s.EffectiveFrom)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    existing.EffectiveTo = DateTime.UtcNow;
                    existing.IsActive = 0;
                    existing.ModifiedDate = DateTime.UtcNow;
                    existing.ModifiedBy = master.CreatedBy ?? existing.ModifiedBy;
                    _context.EmployeeSalaryMasters.Update(existing);
                }

                master.EffectiveFrom = master.EffectiveFrom == default ? DateTime.UtcNow : master.EffectiveFrom;
                master.IsActive = 1;
                master.Del_Flg = "N";
                master.CreatedDate = DateTime.UtcNow;
                master.ModifiedDate = null;

                await _context.EmployeeSalaryMasters.AddAsync(master);
                await _context.SaveChangesAsync();

                return master;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving EmployeeSalaryMaster for EmployeeId {EmployeeId}", master?.EmployeeID);
                return null;
            }
        }

        public async Task<bool> SaveEmployeeSalaryComponentsAsync(int employeeId, List<EmployeePayrollSalaryComponent> components, int payrollMonth, int payrollYear)
        {
            try
            {
                if (employeeId <= 0 || components == null) return false;

                // remove existing components for same employee + payroll period
                var existing = await _context.EmployeePayrollSalaryComponent
                    .Where(c => c.EmployeeID == employeeId && c.PayrollMonth == payrollMonth && c.PayrollYear == payrollYear)
                    .ToListAsync();

                if (existing.Any())
                {
                    _context.EmployeePayrollSalaryComponent.RemoveRange(existing);
                }

                // prepare and insert new components
                var now = DateTime.UtcNow;
                foreach (var comp in components)
                {
                    comp.EmployeeID = employeeId;
                    comp.PayrollMonth = payrollMonth;
                    comp.PayrollYear = payrollYear;
                    comp.IsActive = 1;
                    comp.Del_Flg = "N";
                    comp.CreatedDate = now;
                }

                await _context.EmployeePayrollSalaryComponent.AddRangeAsync(components);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payroll components for EmployeeId {EmployeeId}", employeeId);
                return false;
            }
        }

    }
}
