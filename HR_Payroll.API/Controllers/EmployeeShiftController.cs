using HR_Payroll.Core.Response;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR_Payroll.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeShiftController : ControllerBase
    {
        private readonly ILogger<EmployeeShiftController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmployeeShiftServices _employeeShiftRepository;

        public EmployeeShiftController(IConfiguration configuration,
            ILogger<EmployeeShiftController> logger,
            IEmployeeShiftServices employeeShiftRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _employeeShiftRepository = employeeShiftRepository;

        }

        [HttpGet]
        [Route("GetAllEmployeeShift")]
        public async Task<ActionResult> GetAllEmployeeShift()
        {
            try
            {
                var result = await _employeeShiftRepository.GetShiftsAsync();

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message ?? "Failed to retrieve shift list",
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message ?? "Employee shift list retrieved successfully",
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shift list");
                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "An error occurred while processing your request.",
                    data = new List<object>()
                });
            }
        }

        [HttpGet]
        [Route("GetShiftByCode")]
        public async Task<ActionResult> GetShiftByCode([FromQuery] string shiftCode)
        {
            try
            {
                var result = await _employeeShiftRepository.GetShiftByCodeAsync(shiftCode);

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message ?? "Failed to retrieve employee shift",
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message ?? "Employee shift retrieved successfully",
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee shift");
                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "An error occurred while processing your request.",
                    data = new List<object>()
                });
            }
        }
    }
}
