using HR_Payroll.CommonCases.Utility;
using HR_Payroll.Core.DTO;
using HR_Payroll.Core.Entity;
using HR_Payroll.Core.Model;
using HR_Payroll.Core.Model.Payroll;
using HR_Payroll.Core.Models;
using HR_Payroll.Infrastructure.Concrete;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HR_Payroll.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalaryController : ControllerBase
    {
        private readonly ILogger<SalaryController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISalaryService _salaryService;

        public SalaryController(
            IConfiguration configuration,
            ILogger<SalaryController> logger,
            ISalaryService salaryService)
        {
            _logger = logger;
            _configuration = configuration;
            _salaryService = salaryService;
        }

        [HttpGet]
        [Route("GetSalaryComponents")]
        public async Task<IActionResult> GetSalaryComponents()
        {
            try
            {
                var components = await _salaryService.GetAllSalaryComponentsAsync();
                return Ok(new
                {
                    status = true,
                    message = "Salary components retrieved successfully",
                    data = components
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary components");
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while processing your request.",
                    data = new List<object>()
                });
            }
        }

       
        [HttpPost]
        [Route("SavePayrollInfo")]
        public async Task<IActionResult> SavePayrollInfo([FromBody] EmployeePayrollViewModel model)
        {
            if (model == null || model.EmployeeId == null || model.EmployeeId <= 0)
            {
                return BadRequest(new { status = false, message = "Invalid payload" });
            }

            try
            {
                var master = new EmployeeSalary
                {
                    EmployeeID = model.EmployeeId.Value,
                    Amount = model.SalaryPerMonth,
                    EffectiveFrom = DateTime.UtcNow,
                    IsActive = 1,
                    CreatedBy = "System"
                };

                var saved = await _salaryService.SaveEmployeeSalaryMasterAsync(master);

                if (saved == null)
                {
                    return StatusCode(500, new { status = false, message = "Failed to save payroll info" });
                }

                return Ok(new { status = true, message = "Payroll saved", data = saved });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payroll for employee {EmployeeId}", model.EmployeeId);
                return StatusCode(500, new { status = false, message = "An error occurred while saving payroll" });
            }
        }

        // Save salary breakup components:
        [HttpPost]
        [Route("SaveSalaryBreakup")]
        public async Task<IActionResult> SaveSalaryBreakup([FromBody] EmployeeSalaryBreakupViewModel model)
        {
            if (model == null || model.EmployeeId == null || model.EmployeeId <= 0)
            {
                return BadRequest(new { status = false, message = "Invalid payload" });
            }

            try
            {
                // map DTO -> entity components
                var now = DateTime.UtcNow;
                var payrollMonth = now.Month;
                var payrollYear = now.Year;

                var components = model.SalaryComponents?
                    .Select(s => new EmployeePayrollSalaryComponent
                    {
                        ComponentID = s.ComponentId ?? 0,
                        Amount = s.Amount,
                        PayrollMonth = payrollMonth,
                        PayrollYear = payrollYear,
                        CreatedBy = "System"
                    })
                    .ToList() ?? new List<EmployeePayrollSalaryComponent>();

                var ok = await _salaryService.SaveEmployeeSalaryComponentsAsync(model.EmployeeId.Value, components, payrollMonth, payrollYear);

                if (!ok)
                {
                    return StatusCode(500, new { status = false, message = "Failed to save salary components" });
                }

                return Ok(new { status = true, message = "Salary components saved" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving salary components for employee {EmployeeId}", model.EmployeeId);
                return StatusCode(500, new { status = false, message = "An error occurred while saving salary components" });
            }
        }

        // Payroll Run
        [HttpGet("GetPayrollRunRows")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetPayrollRunRows([FromQuery] string? month, [FromQuery] int? departmentId)
        {
            if (string.IsNullOrWhiteSpace(month))
                return BadRequest(new { status = false, message = "month is required (yyyy-MM)" });

            try
            {
                var rows = await _salaryService.GetPayrollRunRowsAsync(month, departmentId);
                return Ok(new { status = true, message = "OK", data = rows });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payroll run rows");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        [HttpPost("CalculatePayroll")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CalculatePayroll([FromBody] CalculatePayrollRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Month)
                || request.EmployeeIds == null || !request.EmployeeIds.Any())
                return BadRequest(new { status = false, message = "Month and at least one employee are required." });

            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdClaim, out var userId);

                var result = await _salaryService.CalculatePayrollAsync(request.Month, request.EmployeeIds, userId);
                return Ok(new { status = true, message = "Payroll calculated", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating payroll");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        // Salary Slip
        [HttpGet("GetSalarySlip")]
        [Authorize(Roles = "Admin,HR,Manager,Team Lead,Employee")]
        public async Task<IActionResult> GetSalarySlip([FromQuery] int employeeId, [FromQuery] int payrollMonth,
            [FromQuery] int payrollYear)
        {
            if (employeeId <= 0 || payrollMonth is < 1 or > 12 || payrollYear < 2000)
                return BadRequest(new { status = false, message = "Invalid parameters." });

            // Employees may only view their own slip
            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR")
                            || User.IsInRole("Manager") || User.IsInRole("Team Lead");
            if (!isPrivileged)
            {
                var ownIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;
                if (!int.TryParse(ownIdClaim, out var ownId) || ownId != employeeId)
                    return Forbid();
            }

            try
            {
                var slip = await _salaryService.GetSalarySlipAsync(employeeId, payrollMonth, payrollYear);
                if (slip == null)
                    return NotFound(new { status = false, message = "Employee not found." });

                return Ok(new { status = true, message = "OK", data = slip });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salary slip for employee {Id}", employeeId);
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        // Bank Payment
        [HttpGet("GetBankPaymentSummary")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetBankPaymentSummary([FromQuery] string? month)
        {
            if (string.IsNullOrWhiteSpace(month))
                return BadRequest(new { status = false, message = "month is required (yyyy-MM)" });

            try
            {
                var summary = await _salaryService.GetBankPaymentSummaryAsync(month);
                return Ok(new { status = true, message = "OK", data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bank payment summary");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        [HttpPost("MarkPaymentDone")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> MarkPaymentDone([FromBody] MarkPaymentDoneRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PayrollMonth)
                || request.PayrollEmployeeIds == null || !request.PayrollEmployeeIds.Any())
                return BadRequest(new { status = false, message = "Invalid payload." });

            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdClaim, out var userId);

                var ok = await _salaryService.MarkPaymentDoneAsync(
                    request.PayrollMonth, request.PayrollEmployeeIds, userId);

                if (!ok)
                    return StatusCode(500, new { status = false, message = "No matching records found to mark as paid." });

                return Ok(new { status = true, message = "Payment marked as done." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment done");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }
    }
}
