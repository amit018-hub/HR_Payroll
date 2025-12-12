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

        [HttpGet]
        [Route("AdminDashboardData")]
        [Authorize(Roles = "Admin,Employee,HR,Manager,Team Lead")]
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
    }
}
