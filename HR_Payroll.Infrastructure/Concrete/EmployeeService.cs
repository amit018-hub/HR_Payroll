using HR_Payroll.Core.Entity;
using HR_Payroll.Core.Model;
using HR_Payroll.Core.Models;
using HR_Payroll.Infrastructure.Data;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
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

        public async Task<int> SaveAllEmployeeDataAsync(
         EmployeeBasicInfoViewModel basic,
         EmployeePayrollViewModel payroll,
         EmployeeBankViewModel bank,
         List<SalaryComponentViewModel>? components,
         string? profileImage)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    if (basic == null)
                        throw new ArgumentNullException(nameof(basic));

                    int employeeId;
                    Employees employee;

                    if (basic.EmployeeId.HasValue && basic.EmployeeId > 0)
                    {
                        employee = await _context.Employees
                            .FirstOrDefaultAsync(e => e.EmployeeID == basic.EmployeeId)
                            ?? throw new Exception("Employee not found");

                        employee.FirstName = basic.FirstName;
                        employee.LastName = basic.LastName;
                        employee.EmployeeCode = basic.EmployeeCode;
                        employee.DepartmentId = basic.DepartmentId;
                        employee.SubDepartmentId = basic.SubDepartmentId;
                        employee.JoiningDate = basic.JoiningDate;

                        if (!string.IsNullOrWhiteSpace(profileImage))
                            employee.ProfilePic = profileImage;
                        employee.ModifiedBy = basic.CreatedBy;
                        employee.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        employee = new Employees
                        {
                            FirstName = basic.FirstName,
                            LastName = basic.LastName,
                            EmployeeCode = basic.EmployeeCode,
                            DepartmentId = basic.DepartmentId,
                            SubDepartmentId = basic.SubDepartmentId,
                            JoiningDate = basic.JoiningDate,
                            CreatedDate = DateTime.Now,
                            CreatedBy = basic.CreatedBy,
                            ModifiedBy = basic.CreatedBy,
                        };

                        _context.Employees.Add(employee);
                    }

                    await SaveChangesWithCheckAsync("BASIC INFO");
                    employeeId = employee.EmployeeID;

                    // 2️⃣ PAYROLL
                    var payrollDb = await _context.EmployeePayrollSalaryComponent
                        .FirstOrDefaultAsync(p => p.EmployeeID == employeeId);

                    if (payrollDb == null)
                    {
                        payrollDb = new EmployeePayrollSalaryComponent
                        {
                            EmployeeID = employeeId,
                            Amount = payroll.SalaryPerMonth,
                            CreatedDate = DateTime.Now,
                            CreatedBy = basic.CreatedBy,
                            ModifiedBy = basic.CreatedBy,
                        };

                        _context.EmployeePayrollSalaryComponent.Add(payrollDb);
                    }
                    else
                    {
                        payrollDb.Amount = payroll.SalaryPerMonth;
                        payrollDb.ModifiedDate = DateTime.Now;
                        payrollDb.CreatedBy = basic.CreatedBy;
                        payrollDb.ModifiedBy = basic.CreatedBy;
                    }

                    await SaveChangesWithCheckAsync("PAYROLL");

                    // 3️⃣ BANK
                    var bankDb = await _context.EmployeeBank
                        .FirstOrDefaultAsync(b => b.EmployeeId == employeeId);

                    if (bankDb == null)
                    {
                        bankDb = new EmployeeBank
                        {
                            EmployeeId = employeeId,
                            BankName = bank.BankName,
                            BeneficiaryName = bank.BeneficiaryName,
                            AccountNo = bank.AccountNumber,
                            IFSC = bank.IFSCCode,
                            CreatedOn = DateTime.Now,
                            CreatedBy = basic.CreatedBy,
                            ModifiedBy = basic.CreatedBy
                        };

                        _context.EmployeeBank.Add(bankDb);
                    }
                    else
                    {
                        bankDb.BankName = bank.BankName;
                        bankDb.BeneficiaryName = bank.BeneficiaryName;
                        bankDb.AccountNo = bank.AccountNumber;
                        bankDb.IFSC = bank.IFSCCode;
                        bankDb.ModifiedOn = DateTime.Now;
                        bankDb.CreatedBy = basic.CreatedBy;
                        bankDb.ModifiedBy = basic.CreatedBy;
                    }

                    await SaveChangesWithCheckAsync("BANK");

                    // 4️⃣ SALARY COMPONENTS
                    if (components != null && components.Any())
                    {
                        var existing = _context.EmployeeSalaryComponent
                            .Where(c => c.EmployeeID == employeeId);

                        _context.EmployeeSalaryComponent.RemoveRange(existing);
                        await SaveChangesWithCheckAsync("REMOVE COMPONENTS");

                        foreach (var item in components)
                        {
                            _context.EmployeeSalaryComponent.Add(new EmployeeSalaryComponent
                            {
                                EmployeeID = employeeId,
                                ComponentID = item.ComponentId,
                                EffectiveFrom = item.EffectiveFrom,
                                Amount = item.Amount,
                                CreatedDate = DateTime.Now,
                                CreatedBy = basic.CreatedBy,
                                ModifiedBy = basic.CreatedBy,
                                IsActive = 1
                            });
                        }

                        await SaveChangesWithCheckAsync("ADD COMPONENTS");
                    }

                    await transaction.CommitAsync();
                    return employeeId;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // 🔴 THIS IS KEY FOR DEBUGGING
                    throw new Exception(
                        "SaveAllEmployeeDataAsync failed. See inner exception.",
                        ex
                    );
                }
            });
        }

        private async Task SaveChangesWithCheckAsync(string step)
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // 🔴 Breakpoint here — debugger WILL hit
                throw new Exception($"Database update failed at step: {step}", ex);
            }
            catch (Exception ex)
            {
                // 🔴 Breakpoint here — debugger WILL hit
                throw new Exception($"Unexpected error at step: {step}", ex);
            }
        }


    }
}
