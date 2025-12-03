using HR_Payroll.CommonCases.Utility;
using HR_Payroll.Core.Models;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR_Payroll.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmployeeService _employeeService;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(IConfiguration configuration,
            ILogger<EmployeeController> logger,IEmployeeService employeeService , IWebHostEnvironment env)
        {
            _logger = logger;
            _configuration = configuration;
            _employeeService = employeeService;
            _env = env;
        }
        [HttpPost]
        [Route("SaveBasicInfo")]
        public async Task<IActionResult> SaveBasicInfo([FromForm] EmployeeBasicInfoViewModel model)
        {
            if (model == null)
                return BadRequest(new { status = false, message = "Invalid payload" });

            try
            {
                string? profilePath = null;
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    profilePath = ExternalHelper.FileUploadThroughApi(model.ProfilePicture, _env.ContentRootPath, "EmployeeProfile");
                }

                var saved = await _employeeService.SaveBasicInfoAsync(model, profilePath);

                if (saved == null)
                    return StatusCode(500, new { status = false, message = "Failed to save basic info" });

                return Ok(new { status = true, message = "Basic info saved", data = saved });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving basic info");
                return StatusCode(500, new { status = false, message = "An error occurred while saving basic info" });
            }
        }

        [HttpPost]
        [Route("SaveBankDetails")]
        public async Task<IActionResult> SaveBankDetails([FromBody] EmployeeBankViewModel model)
        {
            if (model == null || model.EmployeeId == null || model.EmployeeId <= 0)
                return BadRequest(new { status = false, message = "Invalid payload" });

            try
            {
                var saved = await _employeeService.SaveBankDetailsAsync(model);
                if (saved == null)
                    return StatusCode(500, new { status = false, message = "Failed to save bank details" });

                return Ok(new { status = true, message = "Bank details saved", data = saved });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving bank details for employee {EmployeeId}", model.EmployeeId);
                return StatusCode(500, new { status = false, message = "An error occurred while saving bank details" });
            }
        }


    }
}
