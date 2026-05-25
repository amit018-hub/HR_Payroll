using HR_Payroll.Core.DTO.Dept;
using HR_Payroll.Core.DTO.Leave;
using HR_Payroll.Core.Model.Leave;
using HR_Payroll.Core.Model.Master;
using HR_Payroll.Core.Response;
using HR_Payroll.Web.CommonClients;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HR_Payroll.Web.Controllers
{
    public class PayrollController : Controller
    {
        private readonly CommonAPI_Client _apiClient;
        private readonly ILogger<PayrollController> _logger;
        private readonly IConfiguration _configuration;

        public PayrollController(CommonAPI_Client apiClient,
            ILogger<PayrollController> logger,
            IConfiguration configuration)
        {
            _apiClient = apiClient;
            _logger = logger;
            _configuration = configuration;
        }
        public string _apiBaseUrl => _configuration.GetValue<string>("ApiBaseUrl");

        public async Task<IActionResult> GetLeaveTypes()
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

                var result = await _apiClient.GetAsync<List<LeaveType>>("Leave/GetAllLeaveTypes");

                if (!result.status)
                {
                    _logger.LogWarning("Failed to retrieve leave types: {Message}", result.message);
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Leave types retrieved successfully.",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetLeaveTypes");
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while retrieving leave types."
                });
            }
        }

        public async Task<IActionResult> GetLeaveBalances()
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

                var result = await _apiClient.GetAsync<EmployeeLeaveBalanceResponse>("Leave/GetEmployeeLeaveBalance");

                if (!result.status)
                {
                    _logger.LogWarning("Failed to retrieve leave balances: {Message}", result.message);
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Leave balances retrieved successfully.",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetLeaveBalances");
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while retrieving leave balances."
                });
            }
        }

        public IActionResult ApplyLeave() => View();

        [HttpPost]
        public async Task<IActionResult> ApplyLeave(ApplyLeaveRequestDto req)
        {
            try
            {
                // Get tokens from claims
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { status = false, message = "Missing tokens." });
                }

                // Set tokens for HttpClient
                _apiClient.SetTokens(accessToken, refreshToken);

                // Call the backend API using Dapper/PostAsync
                var result = await _apiClient.PostAsync<object>(
                    "Leave/ApplyLeave", // ✅ correct endpoint
                    req
                );

                if (!result.status)
                {
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Leave applied successfully."
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

        [HttpPost]
        public async Task<IActionResult> LeaveApproveProcess(ProcessLeaveRequest req)
        {
            try
            {
                // Get tokens from claims
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { status = false, message = "Missing tokens." });
                }

                // Set tokens for HttpClient
                _apiClient.SetTokens(accessToken, refreshToken);

                // Call the backend API using Dapper/PostAsync
                var result = await _apiClient.PostAsync<object>(
                    "Leave/LeaveProcess", // ✅ correct endpoint
                    req
                );

                if (!result.status)
                {
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Leave processed successfully."
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

        public async Task<IActionResult> GetLeaveRequests()
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

                var result = await _apiClient.GetAsync<List<EmployeeLeaveRequest>>("Leave/GetAllLeaveRequests");

                if (!result.status)
                {
                    _logger.LogWarning("Failed to retrieve leave requests: {Message}", result.message);
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Leave requests retrieved successfully.",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetLeaveRequestes");
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while retrieving leave requests."
                });
            }
        }

        public async Task<IActionResult> CheckPendingLeave()
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
                var result = await _apiClient.GetAsync<bool>("Leave/CheckPendingLeave");
                if (!result.status)
                {
                    _logger.LogWarning("Failed to check pending leave: {Message}", result.message);
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }
                return Json(new
                {
                    status = true,
                    message = "Check pending leave successfully.",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in CheckPendingLeave");
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while checking pending leave."
                });
            }
        }

        public async Task<IActionResult> GetPendingApprovals()
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

                var result = await _apiClient.GetAsync<List<PendingLeaveDto>>("Leave/PendingLeaveRequests");

                if (!result.status)
                {
                    _logger.LogWarning("Failed to retrieve leave requests: {Message}", result.message);
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Leave requests retrieved successfully.",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetLeaveRequestes");
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while retrieving leave requests."
                });
            }
        }

        public ActionResult PayrollRun()
        {
             return View();
        }


        public ActionResult SalarySlip()
        {
            return View();
        }

        public ActionResult BankPaymentPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoadDepartments()
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                    return Unauthorized(new { status = false, message = "Missing tokens." });

                _apiClient.SetTokens(accessToken, refreshToken);
                var result = await _apiClient.GetAsync<List<object>>("Department/GetAllDepartments");
                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });

                return Json(new { status = true, data = result.data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoadDepartments failed");
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadPayrollEmployees(string? month, int? departmentId)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                    return Unauthorized(new { status = false, message = "Missing tokens." });

                _apiClient.SetTokens(accessToken, refreshToken);

                // Build query string safely
                var q = new Dictionary<string, string?>();
                if (!string.IsNullOrEmpty(month)) q["month"] = month;
                if (departmentId.HasValue) q["departmentId"] = departmentId.Value.ToString();
                var qs = string.Join("&", q.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));
                var path = string.IsNullOrEmpty(qs) ? "Payroll/GetPayrollEmployees" : $"Payroll/GetPayrollEmployees?{qs}";

                var result = await _apiClient.GetAsync<object>(path);
                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });

                return Json(new { status = true, data = result.data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoadPayrollEmployees failed");
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CalculatePayroll([FromBody] object payload)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                    return Unauthorized(new { status = false, message = "Missing tokens." });

                _apiClient.SetTokens(accessToken, refreshToken);
                var result = await _apiClient.PostAsync<object>("Payroll/CalculatePayroll", payload);

                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });

                return Json(new { status = true, data = result.data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CalculatePayroll failed");
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApprovePayroll([FromBody] object payload)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                    return Unauthorized(new { status = false, message = "Missing tokens." });

                _apiClient.SetTokens(accessToken, refreshToken);
                var result = await _apiClient.PostAsync<object>("Payroll/ApprovePayroll", payload);

                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });

                return Json(new { status = true, data = result.data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApprovePayroll failed");
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePayslips([FromBody] object payload)
        {
            try
            {
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                    return Unauthorized(new { status = false, message = "Missing tokens." });

                _apiClient.SetTokens(accessToken, refreshToken);
                var result = await _apiClient.PostAsync<object>("Payroll/GeneratePayslips", payload);

                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });

                return Json(new { status = true, data = result.data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GeneratePayslips failed");
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }

}
