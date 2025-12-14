using HR_Payroll.Core.Response;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR_Payroll.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDashboardService _dashboardRepository;

        public DashboardController(IConfiguration configuration,
            ILogger<DashboardController> logger,
            IDashboardService dashboardRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _dashboardRepository = dashboardRepository;

        }

        private int GetEmployeeIdFromClaims()
        {
            var empIdClaim = HttpContext?.User.FindFirst("EmployeeId")?.Value;
            return int.TryParse(empIdClaim, out int empId) ? empId : 0;
        }
        private int employeeId => GetEmployeeIdFromClaims();

        [HttpGet]
        [Route("AdminDashboardData")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AdminDashboardData()
        {
            try
            {
                var result = await _dashboardRepository.GetAdminDashboardData();

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message ?? "Failed to retrieve data list",
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message ?? "Admin dashboard data retrieved successfully",
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin dashboard data");
                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "An error occurred while processing your request.",
                    data = new List<object>()
                });
            }
        }

        [HttpGet]
        [Route("EmployeeDashboardData")]
        [Authorize(Roles = "Employee,HR,Manager,Team Lead")]
        public async Task<ActionResult> EmployeeDashboardData()
        {
            try
            {
                var result = await _dashboardRepository.GetEmployeeDashboardData(employeeId);

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message ?? "Failed to retrieve data list",
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message ?? "Employee dashboard data retrieved successfully",
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin dashboard data");
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
