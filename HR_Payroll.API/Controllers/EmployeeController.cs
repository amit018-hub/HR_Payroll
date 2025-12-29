using HR_Payroll.CommonCases.Utility;
using HR_Payroll.Core.Model;
using HR_Payroll.Core.Models;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
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
        public class SaveEmployeeRequest
        {
            // JSON strings
            public string Basic { get; set; }
            public string Payroll { get; set; }
            public string Bank { get; set; }
            public string SalaryComponents { get; set; }

            // File
            public IFormFile? ProfilePicture { get; set; }
        }


        [HttpPost("SaveAllEmployeeData")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SaveAllEmployeeData([FromForm] SaveEmployeeRequest model)
        {
            try
            {
                IPAddress? remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
                string ipAddressString = remoteIpAddress?.ToString() ?? "IP not found";

                // Optional: Map to IPv4 if it's an IPv6 loopback (::1) during local debugging
                if (remoteIpAddress != null && remoteIpAddress.IsIPv4MappedToIPv6)
                {
                    remoteIpAddress = remoteIpAddress.MapToIPv4();
                    ipAddressString = remoteIpAddress.ToString();
                }
                
                var basic =
                    JsonSerializer.Deserialize<EmployeeBasicInfoViewModel>(model.Basic);

                var payroll =
                    JsonSerializer.Deserialize<EmployeePayrollViewModel>(model.Payroll);

                var bank =
                    JsonSerializer.Deserialize<EmployeeBankViewModel>(model.Bank);

                var salaryComponents =
                    JsonSerializer.Deserialize<List<SalaryComponentViewModel>>(
                        model.SalaryComponents
                    );
                basic.CreatedBy = ipAddressString;
                string? savedFilePath = null;

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var folder = Path.Combine("wwwroot", "uploads", "employees");
                    Directory.CreateDirectory(folder);

                    var fileName = $"{Guid.NewGuid()}_{model.ProfilePicture.FileName}";
                    var filePath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await model.ProfilePicture.CopyToAsync(stream);

                    savedFilePath = fileName;
                }

                await _employeeService.SaveAllEmployeeDataAsync( basic!, payroll!, bank!, salaryComponents!, savedFilePath );

                return Ok(new
                {
                    status = true,
                    message = "Employee data saved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var list = await _employeeService.GetAllEmployeesAsync();
                return Ok(new { status = true, data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees list");
                return StatusCode(500, new { status = false, message = "Failed to fetch employees" });
            }
        }

        [HttpGet("GetEmployeeDetails/{id:int}")]
        public async Task<IActionResult> GetEmployeeDetails(int id)
        {
            if (id <= 0) return BadRequest(new { status = false, message = "Invalid employee id" });

            try
            {
                var details = await _employeeService.GetEmployeeDetailsAsync(id);
                if (details == null)
                    return NotFound(new { status = false, message = "Employee not found" });

                return Ok(new { status = true, data = details });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee details for {EmployeeId}", id);
                return StatusCode(500, new { status = false, message = "Failed to fetch employee details" });
            }
        }
    }



}

