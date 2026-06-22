using HR_Payroll.CommonCases.Utility;
using HR_Payroll.Core.DTO.Payroll;
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
        private const string EarningType = "Earning";
        private const string EmployerContributionType = "EmployerContribution";

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

        public async Task<EmployeeSalary?> SaveEmployeeSalaryMasterAsync(EmployeeSalary master)
        {
            try
            {
                if (master == null) return null;

                // expire existing active master (if any)
                var existing = await _context.EmployeeSalary
                    .Where(s => s.EmployeeID == master.EmployeeID && (s.IsActive == 1 || s.IsActive == null))
                    .OrderByDescending(s => s.EffectiveFrom)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    existing.IsActive = 0;
                    existing.ModifiedDate = DateTime.UtcNow;
                    existing.ModifiedBy = master.CreatedBy ?? existing.ModifiedBy;
                    _context.EmployeeSalary.Update(existing);
                }

                master.EffectiveFrom = master.EffectiveFrom == default ? DateTime.UtcNow : master.EffectiveFrom;
                master.IsActive = 1;
                master.Del_Flg = "N";
                master.CreatedDate = DateTime.UtcNow;
                master.ModifiedDate = null;

                await _context.EmployeeSalary.AddAsync(master);
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

        // Payroll Run — uses real PayrollRun + PayrollEmployee tables
        public async Task<List<PayrollRunRowDto>> GetPayrollRunRowsAsync(string payrollMonth, int? departmentId)
        {
            try
            {
                // Build employee list
                var empQuery =
                    from e in _context.Employees.AsNoTracking()
                    where e.Del_Flg != "Y" && e.IsActive
                    join d in _context.Departments.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                    from d in dj.DefaultIfEmpty()
                    select new
                    {
                        e.EmployeeID,
                        e.EmployeeCode,
                        e.FirstName,
                        e.LastName,
                        e.DepartmentId,
                        DepartmentName = d != null ? d.DepartmentName : null
                    };

                if (departmentId.HasValue && departmentId.Value > 0)
                    empQuery = empQuery.Where(x => x.DepartmentId == departmentId.Value);

                var employees = await empQuery.ToListAsync();
                if (!employees.Any()) return new List<PayrollRunRowDto>();

                var employeeIds = employees.Select(e => e.EmployeeID).ToList();

                // Find the PayrollRun header for this month (if already created)
                var run = await _context.PayrollRun.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.PayrollMonth == payrollMonth);

                // Load per-employee lines from PayrollEmployee
                var peLines = run == null
                    ? new List<PayrollEmployee>()
                    : await _context.PayrollEmployee.AsNoTracking()
                        .Where(pe => pe.PayrollRunId == run.PayrollRunId &&
                                     pe.EmployeeId != null &&
                                     employeeIds.Contains(pe.EmployeeId.Value))
                        .ToListAsync();

                var peByEmployee = peLines.Where(pe => pe.EmployeeId.HasValue)
                    .ToDictionary(pe => pe.EmployeeId!.Value);

                // Check which employees have salary components configured
                // Use EmployeeSalaryComponent (date-range based, the real table)
                var today = DateTime.Today;
                var componentedEmployeeIds = await _context.EmployeeSalaryComponent.AsNoTracking()
                    .Where(c => employeeIds.Contains(c.EmployeeID)
                                && (c.IsActive == 1 || c.IsActive == null)
                                && c.Del_Flg != "Y"
                                && c.EffectiveFrom <= today
                                && (c.EffectiveTo == null || c.EffectiveTo >= today))
                    .Select(c => c.EmployeeID)
                    .Distinct()
                    .ToListAsync();

                var componentedSet = componentedEmployeeIds.ToHashSet();

                var rows = employees.Select(e =>
                {
                    peByEmployee.TryGetValue(e.EmployeeID, out var pe);
                    var hasComponents = componentedSet.Contains(e.EmployeeID);

                    return new PayrollRunRowDto
                    {
                        EmployeeId = e.EmployeeID,
                        EmployeeCode = e.EmployeeCode,
                        EmployeeName = $"{e.FirstName} {e.LastName}".Trim(),
                        DepartmentName = e.DepartmentName,
                        HasSalaryComponents = hasComponents,
                        PayrollEmployeeId = pe?.PayrollEmployeeId,
                        Gross = pe?.Gross,
                        Deductions = pe?.Deductions,
                        NetPay = pe?.NetPay,
                        Status = pe?.Status ?? (hasComponents ? "Not Calculated" : "No Components")
                    };
                })
                .OrderBy(r => r.EmployeeName)
                .ToList();

                return rows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payroll run rows for {Month}", payrollMonth);
                return new List<PayrollRunRowDto>();
            }
        }

        public async Task<PayrollRunResultDto> CalculatePayrollAsync(string payrollMonth, List<int> employeeIds, int createdByUserId)
        {
            var result = new PayrollRunResultDto { PayrollMonth = payrollMonth };

            try
            {
                if (employeeIds == null || !employeeIds.Any())
                    return result;

                var now = DateTime.UtcNow;
                var today = DateTime.Today;

                // 1. Get or create the PayrollRun header
                var run = await _context.PayrollRun
                    .FirstOrDefaultAsync(r => r.PayrollMonth == payrollMonth);

                if (run == null)
                {
                    run = new PayrollRun
                    {
                        PayrollMonth = payrollMonth,
                        Status = "Draft",
                        CreatedOn = now,
                        CreatedBy = createdByUserId
                    };
                    _context.PayrollRun.Add(run);
                    await _context.SaveChangesAsync(); // need the PK
                }

                result.PayrollRunId = run.PayrollRunId;
                result.Status = run.Status;

                // 2. Load active salary components from EmployeeSalaryComponent (real date-range table)
                var components = await (
                    from c in _context.EmployeeSalaryComponent.AsNoTracking()
                    where employeeIds.Contains(c.EmployeeID)
                          && (c.IsActive == 1 || c.IsActive == null)
                          && c.Del_Flg != "Y"
                          && c.EffectiveFrom <= today
                          && (c.EffectiveTo == null || c.EffectiveTo >= today)
                    join sc in _context.SalaryComponents.AsNoTracking()
                        on c.ComponentID equals sc.ComponentID into scj
                    from sc in scj.DefaultIfEmpty()
                    select new
                    {
                        c.EmployeeID,
                        c.Amount,
                        ComponentType = sc != null ? sc.ComponentType : null
                    }
                ).ToListAsync();

                var componentsByEmp = components.GroupBy(c => c.EmployeeID)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 3. Load employees for name/code
                var employees = await (
                    from e in _context.Employees.AsNoTracking()
                    where employeeIds.Contains(e.EmployeeID)
                    join d in _context.Departments.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                    from d in dj.DefaultIfEmpty()
                    select new
                    {
                        e.EmployeeID,
                        e.EmployeeCode,
                        e.FirstName,
                        e.LastName,
                        DepartmentName = d != null ? d.DepartmentName : null
                    }
                ).ToDictionaryAsync(e => e.EmployeeID);

                // 4. Load existing PayrollEmployee rows for this run
                var existingLines = await _context.PayrollEmployee
                    .Where(pe => pe.PayrollRunId == run.PayrollRunId &&
                                 pe.EmployeeId != null &&
                                 employeeIds.Contains(pe.EmployeeId.Value))
                    .ToListAsync();
                var existingByEmp = existingLines.Where(pe => pe.EmployeeId.HasValue)
                    .ToDictionary(pe => pe.EmployeeId!.Value);

                var rows = new List<PayrollRunRowDto>();

                foreach (var empId in employeeIds)
                {
                    employees.TryGetValue(empId, out var emp);
                    componentsByEmp.TryGetValue(empId, out var comps);

                    var row = new PayrollRunRowDto
                    {
                        EmployeeId = empId,
                        EmployeeCode = emp?.EmployeeCode,
                        EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : "Unknown",
                        DepartmentName = emp?.DepartmentName,
                        HasSalaryComponents = comps != null && comps.Any()
                    };

                    if (!row.HasSalaryComponents)
                    {
                        row.Status = "No Components";
                        rows.Add(row);
                        continue;
                    }

                    // Simple sum — no proration, as agreed
                    row.Gross = comps!
                        .Where(c => string.Equals(c.ComponentType, EarningType, StringComparison.OrdinalIgnoreCase))
                        .Sum(c => c.Amount);
                    row.Deductions = comps!
                        .Where(c => !string.Equals(c.ComponentType, EarningType, StringComparison.OrdinalIgnoreCase)
                                    && !string.Equals(c.ComponentType, EmployerContributionType, StringComparison.OrdinalIgnoreCase))
                        .Sum(c => c.Amount);
                    row.NetPay = row.Gross - row.Deductions;

                    // 5. Upsert PayrollEmployee row — only update if still in Draft/Calculated
                    if (existingByEmp.TryGetValue(empId, out var existing))
                    {
                        if (existing.Status == "Draft" || existing.Status == "Calculated" || existing.Status == null)
                        {
                            existing.Gross = row.Gross;
                            existing.Deductions = row.Deductions;
                            existing.NetPay = row.NetPay;
                            existing.Status = "Calculated";
                        }
                        row.Status = existing.Status ?? "Calculated";
                        row.PayrollEmployeeId = existing.PayrollEmployeeId;
                    }
                    else
                    {
                        var pe = new PayrollEmployee
                        {
                            PayrollRunId = run.PayrollRunId,
                            EmployeeId = empId,
                            Gross = row.Gross,
                            Deductions = row.Deductions,
                            NetPay = row.NetPay,
                            Status = "Calculated"
                        };
                        _context.PayrollEmployee.Add(pe);
                        row.Status = "Calculated";
                    }

                    // 6. Also upsert into legacy Payroll table (for salary slip reads)
                    if (!DateTime.TryParseExact(payrollMonth, "yyyy-MM", null,
                            System.Globalization.DateTimeStyles.None, out var parsedMonth))
                        parsedMonth = DateTime.UtcNow;

                    var legacyPayroll = await _context.Payroll
                        .FirstOrDefaultAsync(p => p.EmployeeID == empId
                                                  && p.PayrollMonth == parsedMonth.Month
                                                  && p.PayrollYear == parsedMonth.Year
                                                  && p.Del_Flg != "Y");
                    if (legacyPayroll == null)
                    {
                        _context.Payroll.Add(new Payroll
                        {
                            EmployeeID = empId,
                            PayrollMonth = parsedMonth.Month,
                            PayrollYear = parsedMonth.Year,
                            GrossEarnings = row.Gross,
                            TotalDeductions = row.Deductions,
                            NetSalary = row.NetPay,
                            IsActive = 1,
                            Del_Flg = "N",
                            CreatedDate = now,
                            CreatedBy = createdByUserId.ToString(),
                            ModifiedDate = now,
                            ModifiedBy = createdByUserId.ToString()
                        });
                    }
                    else if (legacyPayroll.IsActive == 1)
                    {
                        legacyPayroll.GrossEarnings = row.Gross;
                        legacyPayroll.TotalDeductions = row.Deductions;
                        legacyPayroll.NetSalary = row.NetPay;
                        legacyPayroll.ModifiedDate = now;
                        legacyPayroll.ModifiedBy = createdByUserId.ToString();
                    }

                    rows.Add(row);
                }

                await _context.SaveChangesAsync();
                result.Rows = rows;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating payroll for {Month}", payrollMonth);
                return result;
            }
        }

        // Salary Slip — reads from Payroll table + EmployeeSalaryComponent
        public async Task<SalarySlipDto?> GetSalarySlipAsync(int employeeId, int payrollMonth, int payrollYear)
        {
            try
            {
                var emp = await (
                    from e in _context.Employees.AsNoTracking()
                    where e.EmployeeID == employeeId
                    join d in _context.Departments.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                    from d in dj.DefaultIfEmpty()
                    select new
                    {
                        e.EmployeeID,
                        e.EmployeeCode,
                        e.FirstName,
                        e.LastName,
                        e.Designation,
                        e.JoiningDate,
                        DepartmentName = d != null ? d.DepartmentName : null
                    }
                ).FirstOrDefaultAsync();

                if (emp == null) return null;

                var bank = await _context.EmployeeBank.AsNoTracking()
                    .Where(b => b.EmployeeId == employeeId && (b.IsActive == null || b.IsActive == true))
                    .OrderByDescending(b => b.EmployeeBankId)
                    .FirstOrDefaultAsync();

                // Salary components from real EmployeeSalaryComponent table
                // Use a reference date of the first day of the requested payroll month
                var periodDate = new DateTime(payrollYear, payrollMonth, 1);
                var components = await (
                    from c in _context.EmployeeSalaryComponent.AsNoTracking()
                    where c.EmployeeID == employeeId
                          && (c.IsActive == 1 || c.IsActive == null)
                          && c.Del_Flg != "Y"
                          && c.EffectiveFrom <= periodDate
                          && (c.EffectiveTo == null || c.EffectiveTo >= periodDate)
                    join sc in _context.SalaryComponents.AsNoTracking()
                        on c.ComponentID equals sc.ComponentID into scj
                    from sc in scj.DefaultIfEmpty()
                    select new
                    {
                        c.Amount,
                        ComponentName = sc != null ? sc.ComponentName : "Component",
                        ComponentType = sc != null ? sc.ComponentType : null
                    }
                ).ToListAsync();

                var slip = new SalarySlipDto
                {
                    EmployeeID = emp.EmployeeID,
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    Designation = emp.Designation,
                    DepartmentName = emp.DepartmentName,
                    JoiningDate = emp.JoiningDate,
                    MaskedBankAccount = MaskAccountNumber(bank?.AccountNo),
                    PayrollMonth = payrollMonth,
                    PayrollYear = payrollYear
                };

                foreach (var c in components)
                {
                    var line = new PayrollComponentLineDto
                    {
                        ComponentName = c.ComponentName ?? "Component",
                        Amount = c.Amount
                    };
                    if (string.Equals(c.ComponentType, EarningType, StringComparison.OrdinalIgnoreCase))
                        slip.Earnings.Add(line);
                    else if (!string.Equals(c.ComponentType, EmployerContributionType, StringComparison.OrdinalIgnoreCase))
                        slip.Deductions.Add(line);
                }

                slip.TotalEarnings = slip.Earnings.Sum(l => l.Amount);
                slip.TotalDeductions = slip.Deductions.Sum(l => l.Amount);

                // Read persisted NetSalary from Payroll table (authoritative after Calculate)
                var payroll = await _context.Payroll.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.EmployeeID == employeeId
                                              && p.PayrollMonth == payrollMonth
                                              && p.PayrollYear == payrollYear
                                              && p.Del_Flg != "Y");

                if (payroll != null)
                {
                    slip.NetSalary = payroll.NetSalary ?? (slip.TotalEarnings - slip.TotalDeductions);
                    slip.TotalEarnings = payroll.GrossEarnings ?? slip.TotalEarnings;
                    slip.TotalDeductions = payroll.TotalDeductions ?? slip.TotalDeductions;
                }
                else
                {
                    slip.NetSalary = slip.TotalEarnings - slip.TotalDeductions;
                }

                slip.NetSalaryInWords = NumberToWordsHelper.ConvertRupeesToWords(slip.NetSalary);

                // Determine status from PayrollEmployee via PayrollRun
                var monthStr = $"{payrollYear:D4}-{payrollMonth:D2}";
                var peStatus = await (
                    from r in _context.PayrollRun.AsNoTracking()
                    where r.PayrollMonth == monthStr
                    join pe in _context.PayrollEmployee.AsNoTracking()
                        on r.PayrollRunId equals pe.PayrollRunId
                    where pe.EmployeeId == employeeId
                    select pe.Status
                ).FirstOrDefaultAsync();

                slip.Status = peStatus ?? (components.Any() ? "Not Calculated" : "No Components");
                return slip;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building salary slip for employee {Id} {Month}/{Year}",
                    employeeId, payrollMonth, payrollYear);
                return null;
            }
        }

        // ---------------------------------------------------------------
        // Bank Payment — uses PayrollRun + PayrollEmployee + EmployeeBank
        // ---------------------------------------------------------------

        public async Task<BankPaymentSummaryDto> GetBankPaymentSummaryAsync(string payrollMonth)
        {
            var summary = new BankPaymentSummaryDto { PayrollMonth = payrollMonth };
            try
            {
                var run = await _context.PayrollRun.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.PayrollMonth == payrollMonth);

                if (run == null) return summary;

                var lines = await (
                    from pe in _context.PayrollEmployee.AsNoTracking()
                    where pe.PayrollRunId == run.PayrollRunId && pe.EmployeeId != null
                    join e in _context.Employees.AsNoTracking() on pe.EmployeeId equals e.EmployeeID
                    select new
                    {
                        pe.PayrollEmployeeId,
                        pe.EmployeeId,
                        e.EmployeeCode,
                        e.FirstName,
                        e.LastName,
                        pe.NetPay,
                        pe.Status
                    }
                ).ToListAsync();

                if (!lines.Any()) return summary;

                var employeeIds = lines.Where(l => l.EmployeeId.HasValue)
                    .Select(l => l.EmployeeId!.Value).ToList();

                var banks = await _context.EmployeeBank.AsNoTracking()
                    .Where(b => b.EmployeeId != null
                                && employeeIds.Contains(b.EmployeeId.Value)
                                && (b.IsActive == null || b.IsActive == true))
                    .ToListAsync();

                var bankByEmp = banks
                    .Where(b => b.EmployeeId.HasValue)
                    .GroupBy(b => b.EmployeeId!.Value)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(b => b.EmployeeBankId).First());

                foreach (var l in lines)
                {
                    if (!l.EmployeeId.HasValue) continue;
                    bankByEmp.TryGetValue(l.EmployeeId.Value, out var bank);
                    var hasBankDetails = bank != null && !string.IsNullOrWhiteSpace(bank.AccountNo);

                    summary.Rows.Add(new BankPaymentRowDto
                    {
                        PayrollEmployeeId = l.PayrollEmployeeId,
                        EmployeeId = l.EmployeeId.Value,
                        EmployeeCode = l.EmployeeCode,
                        EmployeeName = $"{l.FirstName} {l.LastName}".Trim(),
                        BankName = bank?.BankName,
                        AccountNo = MaskAccountNumber(bank?.AccountNo),
                        IFSC = bank?.IFSC,
                        NetPay = l.NetPay ?? 0,
                        Status = l.Status ?? "Calculated",
                        HasBankDetails = hasBankDetails
                    });
                }

                summary.Rows = summary.Rows.OrderBy(r => r.EmployeeName).ToList();
                summary.TotalEmployees = summary.Rows.Count;
                summary.TotalAmount = summary.Rows.Sum(r => r.NetPay);
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building bank payment summary for {Month}", payrollMonth);
                return summary;
            }
        }

        public async Task<bool> MarkPaymentDoneAsync(string payrollMonth, List<int> payrollEmployeeIds, int modifiedByUserId)
        {
            try
            {
                if (payrollEmployeeIds == null || !payrollEmployeeIds.Any()) return false;

                var run = await _context.PayrollRun
                    .FirstOrDefaultAsync(r => r.PayrollMonth == payrollMonth);
                if (run == null) return false;

                var lines = await _context.PayrollEmployee
                    .Where(pe => pe.PayrollRunId == run.PayrollRunId
                                 && payrollEmployeeIds.Contains(pe.PayrollEmployeeId))
                    .ToListAsync();

                if (!lines.Any()) return false;

                var now = DateTime.UtcNow;
                foreach (var line in lines)
                {
                    line.Status = "Paid";
                    line.PaidOn = now;
                }

                // Update PayrollRun header status if all lines are now paid
                var allLinesForRun = await _context.PayrollEmployee
                    .Where(pe => pe.PayrollRunId == run.PayrollRunId)
                    .ToListAsync();

                if (allLinesForRun.All(pe => pe.Status == "Paid"))
                    run.Status = "Paid";

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment done for {Month}", payrollMonth);
                return false;
            }
        }

        private static string? MaskAccountNumber(string? accountNo)
        {
            if (string.IsNullOrWhiteSpace(accountNo) || accountNo.Length <= 4)
                return accountNo;
            return new string('X', accountNo.Length - 4) + accountNo[^4..];
        }
    }
}
