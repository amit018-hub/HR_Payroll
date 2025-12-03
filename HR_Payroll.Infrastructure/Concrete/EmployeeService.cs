using HR_Payroll.Core.Entity;
using HR_Payroll.Core.Models;
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
    public class EmployeeService:IEmployeeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(AppDbContext context, ILogger<EmployeeService> logger)
        {
            _logger = logger;
            _context = context;
        }


        public async Task<Employees?> SaveBasicInfoAsync(EmployeeBasicInfoViewModel model, string? profilePicPath = null)
        {
            try
            {
                if (model == null) return null;

                Employees entity;
                if (model.EmployeeId.HasValue && model.EmployeeId.Value > 0)
                {
                    entity = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeId.Value)
                             ?? new Employees();
                    // update fields
                    entity.EmployeeCode = model.EmployeeCode ?? entity.EmployeeCode;
                    entity.FirstName = model.FirstName ?? entity.FirstName;
                    entity.LastName = model.LastName ?? entity.LastName;
                    entity.DepartmentId = model.DepartmentId ?? entity.DepartmentId;
                    entity.SubDepartmentId = model.SubDepartmentId ?? entity.SubDepartmentId;
                    entity.JoiningDate = model.JoiningDate ?? entity.JoiningDate;
                    // ProfilePic path
                    if (!string.IsNullOrEmpty(profilePicPath)) entity.ProfilePic = profilePicPath;
                    entity.ModifiedDate = DateTime.UtcNow;
                    entity.ModifiedBy = model.FirstName ?? "System";
                    _context.Employees.Update(entity);
                }
                else
                {
                    entity = new Employees
                    {
                        EmployeeCode = model.EmployeeCode,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DepartmentId = model.DepartmentId ?? 0,
                        SubDepartmentId = model.SubDepartmentId ?? 0,
                        JoiningDate = model.JoiningDate ?? DateTime.UtcNow,
                        ProfilePic = profilePicPath,
                        IsActive = true,
                        Del_Flg = "N",
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = model.FirstName ?? "System"
                    };
                    await _context.Employees.AddAsync(entity);
                }

                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving basic info for employee {EmployeeId}", model?.EmployeeId);
                return null;
            }
        }

        public async Task<EmployeeBank?> SaveBankDetailsAsync(EmployeeBankViewModel model)
        {
            try
            {
                if (model == null || model.EmployeeId == null || model.EmployeeId <= 0) return null;

                var existing = await _context.EmployeeBank
                    .FirstOrDefaultAsync(b => b.EmployeeId == model.EmployeeId.Value);

                if (existing != null)
                {
                    existing.BeneficiaryName = model.BeneficiaryName ?? existing.BeneficiaryName;
                    existing.BankName = model.BankName ?? existing.BankName;
                    existing.AccountNo = model.AccountNumber ?? existing.AccountNo;
                    existing.IFSC = model.IFSCCode ?? existing.IFSC;
                    existing.ModifiedOn = DateTime.UtcNow;
                    existing.ModifiedBy = "System";
                    _context.EmployeeBank.Update(existing);
                    await _context.SaveChangesAsync();
                    return existing;
                }

                var bank = new EmployeeBank
                {
                    EmployeeId = model.EmployeeId.Value,
                    BeneficiaryName = model.BeneficiaryName,
                    BankName = model.BankName,
                    AccountNo = model.AccountNumber,
                    IFSC = model.IFSCCode,
                    IsActive = true,
                    //D = "N",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                await _context.EmployeeBank.AddAsync(bank);
                await _context.SaveChangesAsync();
                return bank;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving bank details for employee {EmployeeId}", model?.EmployeeId);
                return null;
            }
        }
    }
}
