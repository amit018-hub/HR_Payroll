using HR_Payroll.Core.Entity;
using HR_Payroll.Core.Model;
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

        public async Task<int> SaveAllEmployeeDataAsync(
        EmployeeBasicInfoViewModel basic,
        EmployeePayrollViewModel payroll,
        EmployeeBankViewModel bank,
        List<SalaryComponentViewModel>? components,
        string? profileImage)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int employeeId;

                // 1️⃣ Save BASIC INFO (Insert or Update)
                Employees employee;

                if (basic.EmployeeId.HasValue && basic.EmployeeId > 0)
                {
                    employee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.EmployeeID == basic.EmployeeId);

                    if (employee == null)
                        throw new Exception("Employee not found.");

                    // Update fields
                    employee.FirstName = basic.FirstName;
                    employee.LastName = basic.LastName;
                    employee.EmployeeCode = basic.EmployeeCode;
                    employee.DepartmentId = basic.DepartmentId;
                    employee.SubDepartmentId = basic.SubDepartmentId;
                    //employee.StateID = basic.State;
                    employee.JoiningDate = basic.JoiningDate;
                    //employee.ReportingTo = basic.ReportingTo;
                    //employee.SourceOfHire = basic.SourceOfHire;
                    //employee.Interviewer = basic.Interviewer;
                    //employee.AttendanceRules = basic.AttendanceRules;
                    //employee.EmploymentStatus = basic.EmploymentStatus;
                    //employee.MaritalStatus = basic.MaritalStatus;
                    //employee.AadharNo = basic.AadharNo;
                    //employee.PANNo = basic.PANNo;
                    //employee.PFNo = basic.PFNo;
                    //employee.UANNo = basic.UANNo;
                    //employee.ESINo = basic.ESINo;
                    //employee.NoticePeriod = basic.NoticePeriod;

                    if (!string.IsNullOrWhiteSpace(profileImage))
                        employee.ProfilePic = profileImage;

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
                       // State = basic.State,
                        JoiningDate = basic.JoiningDate,
                       // ReportingTo = basic.ReportingTo,
                       // SourceOfHire = basic.SourceOfHire,
                       // Interviewer = basic.Interviewer,
                       // AttendanceRules = basic.AttendanceRules,
                       // EmploymentStatus = basic.EmploymentStatus,
                       // MaritalStatus = basic.MaritalStatus,
                       // AadharNo = basic.AadharNo,
                       // PANNo = basic.PANNo,
                       // PFNo = basic.PFNo,
                       // UANNo = basic.UANNo,
                       // ESINo = basic.ESINo,
                       // NoticePeriod = basic.NoticePeriod,
                       // ProfilePicture = profileImage,
                       // CreatedDate = DateTime.Now
                    };

                    _context.Employees.Add(employee);
                }

                await _context.SaveChangesAsync();
                employeeId = employee.EmployeeID;

                // 2️⃣ Save PAYROLL
                //var payrollDb = await _context.EmployeePayrollSalaryComponent
                //    .FirstOrDefaultAsync(p => p.EmployeeID == employeeId);

                //if (payrollDb == null)
                //{
                //    payrollDb = new EmployeePayroll
                //    {
                //        EmployeeID = employeeId,
                //        BasicSalary = payroll.BasicSalary,
                //        HRA = payroll.HRA,
                //        OtherAllowance = payroll.OtherAllowance,
                //        CreatedDate = DateTime.Now
                //    };

                //    _context.EmployeePayroll.Add(payrollDb);
                //}
                //else
                //{
                //    payrollDb.BasicSalary = payroll.BasicSalary;
                //    payrollDb.HRA = payroll.HRA;
                //    payrollDb.OtherAllowance = payroll.OtherAllowance;
                //    payrollDb.ModifiedDate = DateTime.Now;
                //}

                //await _context.SaveChangesAsync();

                // 3️⃣ Save BANK DETAILS
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
                        CreatedOn = DateTime.Now
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
                }

                await _context.SaveChangesAsync();

                // 4️⃣ Save SALARY COMPONENTS
                if (components != null && components.Count > 0)
                {
                    // Remove previous
                    var existing = _context.EmployeeSalaryComponents
                        .Where(c => c.EmployeeID == employeeId);

                    _context.EmployeeSalaryComponents.RemoveRange(existing);
                    await _context.SaveChangesAsync();

                    // Add new component rows
                    foreach (var item in components)
                    {
                        var comp = new EmployeeSalaryComponent
                        {
                            EmployeeID = employeeId,
                            ComponentID = item.ComponentID,
                            EffectiveFrom = item.EffectiveFrom,
                            Amount = item.Amount,
                            CreatedDate = DateTime.Now,
                            IsActive = 1
                        };

                        _context.EmployeeSalaryComponents.Add(comp);
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return employeeId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
