using HR_Payroll.CommonCases.Utility;
using HR_Payroll.Core.Model;
using HR_Payroll.Core.Models;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        //[HttpPost]
        //[Route("SaveBasicInfo")]
        //public async Task<IActionResult> SaveBasicInfo([FromForm] EmployeeBasicInfoViewModel model, [FromForm] EmployeePayrollViewModel? payroll, [FromForm] EmployeeBankViewModel? bank, [FromForm] string? SalaryComponents)
        //{
        //    if (model == null)
        //        return BadRequest(new { status = false, message = "Invalid payload" });

        //    try
        //    {
        //        string? profilePath = null;
        //        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        //        {
        //            profilePath = ExternalHelper.FileUploadThroughApi(model.ProfilePicture, _env.ContentRootPath, "EmployeeProfile");
        //        }

        //        var saved = await _employeeService.SaveBasicInfoAsync(model, profilePath);

        //        if (saved == null)
        //            return StatusCode(500, new { status = false, message = "Failed to save basic info" });

        //        return Ok(new { status = true, message = "Basic info saved", data = saved });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error saving basic info");
        //        return StatusCode(500, new { status = false, message = "An error occurred while saving basic info" });
        //    }
        //}

        //[HttpPost("SaveAllEmployeeData")]
        //public async Task<IActionResult> SaveAllEmployeeData([FromForm] EmployeeBasicInfoViewModel basic, [FromForm] string? payroll, [FromForm] string? bank, [FromForm] string? SalaryComponents, [FromForm] IFormFile? ProfilePicture)
        //{
        //    try
        //    {
        //        // 🔹 Deserialize payroll JSON
        //        EmployeePayrollViewModel? payrollObj = null;
        //        if (!string.IsNullOrWhiteSpace(payroll))
        //        {
        //            payrollObj = JsonSerializer.Deserialize<EmployeePayrollViewModel>(payroll);
        //        }

        //        // 🔹 Deserialize bank JSON
        //        EmployeeBankViewModel? bankObj = null;
        //        if (!string.IsNullOrWhiteSpace(bank))
        //        {
        //            bankObj = JsonSerializer.Deserialize<EmployeeBankViewModel>(bank);
        //        }

        //        // 🔹 Deserialize salary components list
        //        List<SalaryComponentViewModel>? salaryComponentsList = null;
        //        if (!string.IsNullOrWhiteSpace(SalaryComponents))
        //        {
        //            salaryComponentsList = JsonSerializer.Deserialize<List<SalaryComponentViewModel>>(SalaryComponents);
        //        }

        //        // 🔹 Handle profile picture upload
        //        string? savedFilePath = null;
        //        if (ProfilePicture != null && ProfilePicture.Length > 0)
        //        {
        //            var folder = Path.Combine("wwwroot", "uploads", "employees");
        //            if (!Directory.Exists(folder))
        //                Directory.CreateDirectory(folder);

        //            var fileName = $"{Guid.NewGuid()}_{ProfilePicture.FileName}";
        //            var filePath = Path.Combine(folder, fileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await ProfilePicture.CopyToAsync(stream);
        //            }

        //            savedFilePath = fileName;  // store in DB
        //        }

        //        await _employeeService.SaveAllEmployeeDataAsync(basic, payrollObj, bankObj, salaryComponentsList, savedFilePath);

        //        return Ok(new
        //        {
        //            status = true,
        //            message = "Employee data saved successfully",
        //            profileImage = savedFilePath
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new
        //        {
        //            status = false,
        //            message = ex.Message
        //        });
        //    }
        //}

    }
}
