using HR_Payroll.CommonCases.Utility;
using HR_Payroll.Core.Entity;
using HR_Payroll.Core.Model;
using HR_Payroll.Core.Models;
using HR_Payroll.Infrastructure.Concrete;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                var master = new EmployeeSalaryMaster
                {
                    EmployeeID = model.EmployeeId.Value,
                    Salary = model.SalaryPerMonth,
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


    }
}
