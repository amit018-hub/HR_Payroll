using HR_Payroll.Core.DTO.TimeSheet;
using HR_Payroll.Core.Model.TimeSheet;
using HR_Payroll.Core.Services;
using HR_Payroll.Web.CommonClients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace HR_Payroll.Web.Controllers
{
    public class TimeSheetController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CommonAPI_Client _apiClient;
        private readonly ILogger<TimeSheetController> _logger;
        private readonly IConfiguration _configuration;
        private readonly AuthCookieService _authCookieService;

        public TimeSheetController(
            IHttpClientFactory httpClientFactory,
            CommonAPI_Client apiClient,
            ILogger<TimeSheetController> logger,
            AuthCookieService authCookieService,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiClient = apiClient;
            _logger = logger;
            _authCookieService = authCookieService;
            _configuration = configuration;
        }

        public string _apiBaseUrl => _configuration.GetValue<string>("ApiBaseUrl");

        #region --------------------- Timesheet APIs ---------------------

        // GET TIMESHEET
        public async Task<IActionResult> GetTimesheet(int weekOffset = 0)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return RedirectToAction("Login", "Account");
                }

                _apiClient.SetTokens(accessToken, refreshToken);

                var result = await _apiClient.GetAsync<TimesheetViewModel>(
                    "Timesheet/GetTimesheet",
                    new Dictionary<string, string>
                    {
                        { "weekOffset", weekOffset.ToString() }
                    });

                if (!result.status)
                {
                    return Json(new { status = false, message = result.message });
                }

                return Json(new
                {
                    status = true,
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching timesheet");

                return Json(new
                {
                    status = false,
                    message = "Error fetching timesheet"
                });
            }
        }

        // SAVE TIMESHEET
        [HttpPost]
        public async Task<IActionResult> SaveTimesheet([FromBody] SaveTimesheetRequest req)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { status = false, message = "Missing tokens." });
                }

                _apiClient.SetTokens(accessToken, refreshToken);

                var result = await _apiClient.PostAsync<object>("Timesheet/save", req);

                if (!result.status)
                {
                    return Json(new { status = false, message = result.message });
                }

                return Json(new
                {
                    status = true,
                    message = "Timesheet saved successfully",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving timesheet");

                return Json(new
                {
                    status = false,
                    message = "Error saving timesheet"
                });
            }
        }

        // SUBMIT TIMESHEET
        [HttpPost]
        public async Task<IActionResult> SubmitTimesheet([FromBody] SubmitTimesheetRequest req)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { status = false, message = "Missing tokens." });
                }

                _apiClient.SetTokens(accessToken, refreshToken);

                var result = await _apiClient.PostAsync<object>("Timesheet/submit", req);

                if (!result.status)
                {
                    return Json(new { status = false, message = result.message });
                }

                return Json(new
                {
                    status = true,
                    message = "Timesheet submitted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting timesheet");

                return Json(new
                {
                    status = false,
                    message = "Error submitting timesheet"
                });
            }
        }

        // DELETE ROW
        [HttpPost]
        public async Task<IActionResult> DeleteRow(int rowId)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { status = false, message = "Missing tokens." });
                }

                _apiClient.SetTokens(accessToken, refreshToken);

                var result = await _apiClient.PostAsync<object>($"Timesheet/row/{rowId}", null);

                if (!result.status)
                {
                    return Json(new { status = false, message = result.message });
                }

                return Json(new
                {
                    status = true,
                    message = "Row deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting row");

                return Json(new
                {
                    status = false,
                    message = "Error deleting row"
                });
            }
        }

        // RESET ROW
        [HttpPost]
        public async Task<IActionResult> ResetRow(int rowId)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { status = false, message = "Missing tokens." });
                }

                _apiClient.SetTokens(accessToken, refreshToken);

                var result = await _apiClient.PostAsync<object>($"Timesheet/row/{rowId}/reset", null);

                if (!result.status)
                {
                    return Json(new { status = false, message = result.message });
                }

                return Json(new
                {
                    status = true,
                    message = "Row reset successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting row");

                return Json(new
                {
                    status = false,
                    message = "Error resetting row"
                });
            }
        }

        // HISTORY
        public async Task<IActionResult> GetHistory(int timesheetId)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { status = false, message = "Missing tokens." });
                }

                _apiClient.SetTokens(accessToken, refreshToken);

                var result = await _apiClient.GetAsync<IEnumerable<TimesheetAuditDto>>($"Timesheet/{timesheetId}/history");

                if (!result.status)
                {
                    return Json(new { status = false, message = result.message });
                }

                return Json(new
                {
                    status = true,
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching history");

                return Json(new
                {
                    status = false,
                    message = "Error fetching history"
                });
            }
        }

        #endregion
    }
}