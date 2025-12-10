using HR_Payroll.Core.Model.Dashboard;
using HR_Payroll.Core.Model.Leave;
using HR_Payroll.Web.CommonClients;
using Microsoft.AspNetCore.Mvc;

namespace HR_Payroll.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly CommonAPI_Client _apiClient;
        private readonly ILogger<DashboardController> _logger;
        private readonly IConfiguration _configuration;

        public DashboardController(CommonAPI_Client apiClient,
            ILogger<DashboardController> logger,
            IConfiguration configuration)
        {
            _apiClient = apiClient;
            _logger = logger;
            _configuration = configuration;
        }
        public string _apiBaseUrl => _configuration.GetValue<string>("ApiBaseUrl");

        public IActionResult AdminDashboard() => View();

        public async Task<IActionResult> GetAdminDashboard()
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new
                    {
                        status = false,
                        message = "Missing access or refresh token."
                    });
                }

                _apiClient.SetTokens(accessToken, refreshToken);

                var result = await _apiClient.GetAsync<DashboardViewModel>("Dashboard/AdminDashboardData");

                if (!result.status)
                {
                    _logger.LogWarning("Failed to retrieve dashboard data: {Message}", result.message); // updated log message
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Dashboard data retrieved successfully.",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetAdminDashboard"); // updated log message
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while retrieving the dashboard data."
                });
            }
        }
      
    }
}
