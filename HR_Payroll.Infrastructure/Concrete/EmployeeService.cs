using HR_Payroll.Core.DTO;
using HR_Payroll.Core.Entity;
using HR_Payroll.Core.Model;
using HR_Payroll.Core.Model.Master;
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
    public class EmployeeService : IEmployeeService
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
                string? newEmployeeCode = null;
                if (!basic.EmployeeId.HasValue || basic.EmployeeId <= 0)
                {
                    newEmployeeCode = await GenerateUniqueEmployeeIdAsync();
                }

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
                        employee.DepartmentId = basic.DepartmentId;
                        employee.SubDepartmentId = basic.SubDepartmentId;
                        employee.JoiningDate = basic.JoiningDate;
                        employee.Reporting_To = basic.ReportingTo;
                        employee.Interviewer = basic.Interviewer;
                        employee.AttendanceRules = basic.AttendanceRules;
                        employee.MaritalStatus = basic.MaritalStatus;
                        employee.AadhaarNo = basic.AadharNo;
                        employee.PANNo = basic.PANNo;
                        employee.PFNo = basic.PFNo;
                        employee.UANNo = basic.UANNo;
                        employee.ESINo = basic.ESINo;
                        employee.NoticePeriod = basic.NoticePeriod;
                        if (!string.IsNullOrWhiteSpace(profileImage))
                            employee.ProfilePic = profileImage;
                        employee.ModifiedBy = basic.CreatedBy;
                        employee.ModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        int nextNumber = await _context.Employees.MaxAsync(e => (int?)e.EmployeeID) ?? 0;
                        var user = new Users
                        {
                            UserName = $"Employee-{nextNumber + 1}",       // Login ID
                            Email = basic.Email,                     // Assuming available
                            MobileNumber = basic.MobileNumber,       // Assuming available
                            PasswordHash = basic.Password,
                            IsEmailVerified = false,
                            IsMobileVerified = false,
                            IsTwoFactorEnabled = false,
                            Status = "Active",
                            Del_Flg = "N",
                            CreatedDate = DateTime.Now,
                            CreatedBy = basic.CreatedBy,
                            ModifiedBy = basic.CreatedBy,
                            LoggedIn = false,
                            AccountLocked = false,
                            LoginFailureAttempt = 0,
                            AccountStatusID = 1,
                            UserTypeId = 3,                          // Employee
                            EmailOtpRequiredForLogin = 0,
                            MobileOtpRequiredForLogin = 0
                        };

                        _context.Users.Add(user);
                        await SaveChangesWithCheckAsync("USER INSERT");

                        employee = new Employees
                        {
                            UserID = user.UserID,
                            FirstName = basic.FirstName,
                            LastName = basic.LastName,
                            EmployeeCode = newEmployeeCode,
                            DepartmentId = basic.DepartmentId,
                            SubDepartmentId = basic.SubDepartmentId,
                            JoiningDate = basic.JoiningDate,
                            Reporting_To = basic.ReportingTo,
                            Interviewer = basic.Interviewer,
                            AttendanceRules = basic.AttendanceRules,
                            MaritalStatus = basic.MaritalStatus,
                            AadhaarNo = basic.AadharNo,
                            PANNo = basic.PANNo,
                            PFNo = basic.PFNo,
                            UANNo = basic.UANNo,
                            ESINo = basic.ESINo,
                            NoticePeriod = basic.NoticePeriod,
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

        public async Task<string> GenerateUniqueEmployeeIdAsync()
        {
            const string prefix = "EMP";

            using var transaction = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                // Get last EmployeeId safely
                var lastEmployeeCode = await _context.Employees
                    .OrderByDescending(e => e.EmployeeID)
                    .Select(e => e.EmployeeCode)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;


                if (!string.IsNullOrEmpty(lastEmployeeCode) && lastEmployeeCode.StartsWith(prefix))
                {
                    string numericPart = lastEmployeeCode.Substring(prefix.Length);
                    if (int.TryParse(numericPart, out int parsed))
                    {
                        nextNumber = parsed + 1;
                    }
                }

                var newEmployeeId = $"{prefix}-{nextNumber:D4}";

                await transaction.CommitAsync();
                return newEmployeeId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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

        public async Task<List<AssignEmployeeListModel>> GetAllEmployeesAsync()
        {
            try
            {
                // Pull projection into memory first so we can compute SlNo and TotalRecords,
                // avoiding EF Core translation issues (Select overload with index isn't supported).
                var items = await (
                                    from e in _context.Employees.AsNoTracking()
                                    join d in _context.Departments.AsNoTracking() on e.DepartmentId equals d.DepartmentId into deptJoin
                                    from d in deptJoin.DefaultIfEmpty()
                                    join sd in _context.SubDepartments.AsNoTracking() on e.SubDepartmentId equals sd.SubDepartmentId into subDeptJoin
                                    from sd in subDeptJoin.DefaultIfEmpty()
                                    orderby e.EmployeeID descending
                                    select new
                                    {
                                        e.EmployeeID,
                                        FirstName = e.FirstName ?? string.Empty,
                                        LastName = e.LastName ?? string.Empty,
                                        e.EmployeeCode,
                                        DepartmentName = d != null ? d.DepartmentName : null,
                                        SubDepartmentName = sd != null ? sd.SubDepartmentName : null,
                                        Manager = e.Reporting_To,
                                        e.JoiningDate,
                                        e.IsActive
                                    })
                                    .ToListAsync();

                var total = items.Count;
                var result = items.Select((x, i) => new AssignEmployeeListModel
                {
                    SlNo = i + 1,
                    TotalRecords = total,
                    EmployeeID = x.EmployeeID,
                    EmployeeName = $"{x.FirstName} {x.LastName}".Trim(),
                    Department = x.DepartmentName,
                    SubDepartment = x.SubDepartmentName,
                    TeamLead = null,
                    Manager = x.Manager,
                    EmployeeCode = x.EmployeeCode,
                    IsActive = x.IsActive,
                    JoiningDate = x.JoiningDate
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee list");
                return new List<AssignEmployeeListModel>();
            }
        }

        public async Task<EmployeeDetailsModel?> GetEmployeeDetailsAsync(int employeeId)
        {
            if (employeeId <= 0) return null;

            try
            {
                var basicWithNames = await (
                    from e in _context.Employees.AsNoTracking()
                    where e.EmployeeID == employeeId
                    join d in _context.Departments.AsNoTracking() on e.DepartmentId equals d.DepartmentId into deptJoin
                    from d in deptJoin.DefaultIfEmpty()
                    join sd in _context.SubDepartments.AsNoTracking() on e.SubDepartmentId equals sd.SubDepartmentId into subDeptJoin
                    from sd in subDeptJoin.DefaultIfEmpty()
                    select new EmployeeBasicDto
                    {
                        EmployeeID = e.EmployeeID,
                        EmployeeCode = e.EmployeeCode,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        DepartmentId = e.DepartmentId,
                        DepartmentName = d != null ? d.DepartmentName : null,
                        SubDepartmentId = e.SubDepartmentId,
                        SubDepartmentName = sd != null ? sd.SubDepartmentName : null,
                        Reporting_To = e.Reporting_To,
                        JoiningDate = e.JoiningDate,
                        ProfilePic = e.ProfilePic,
                        AadhaarNo = e.AadhaarNo,
                        PANNo = e.PANNo,
                        PFNo = e.PFNo,
                        UANNo = e.UANNo,
                        ESINo = e.ESINo
                    })
                    .FirstOrDefaultAsync();

                if (basicWithNames == null) return null;

                var bank = await _context.EmployeeBank
                    .AsNoTracking()
                    .Where(b => b.EmployeeId == employeeId)
                    .Select(b => new EmployeeBankDto
                    {
                        BeneficiaryName = b.BeneficiaryName,
                        BankName = b.BankName,
                        AccountNo = b.AccountNo,
                        IFSC = b.IFSC
                    })
                    .FirstOrDefaultAsync();

                var payroll = await _context.EmployeeSalary
                    .AsNoTracking()
                    .Where(p => p.EmployeeID == employeeId)
                    .OrderByDescending(p => p.EffectiveFrom)
                    .Select(p => new PayrollDto
                    {
                        Amount = p.Amount,
                        EffectiveFrom = p.EffectiveFrom,
                        PayrollMonth = p.PayrollMonth,
                        PayrollYear = p.PayrollYear
                    })
                    .FirstOrDefaultAsync();

                var components = await (
                                        from c in _context.EmployeeSalaryComponent.AsNoTracking()
                                        join sc in _context.SalaryComponents.AsNoTracking()
                                            on c.ComponentID equals sc.ComponentID into scJoin
                                        from sc in scJoin.DefaultIfEmpty()
                                        where c.EmployeeID == employeeId
                                        orderby c.EffectiveFrom descending
                                        select new SalaryComponentDto
                                        {
                                            ComponentId = c.ComponentID,
                                            ComponentName = sc != null ? sc.ComponentName : null,
                                            Amount = c.Amount,
                                            EffectiveFrom = c.EffectiveFrom
                                        })
                                        .ToListAsync();

                return new EmployeeDetailsModel
                {
                    Basic = basicWithNames,
                    Bank = bank,
                    Payroll = payroll,
                    Components = components
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee details for {EmployeeId}", employeeId);
                return null;
            }
        }

    }
}
