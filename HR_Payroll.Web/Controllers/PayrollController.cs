using HR_Payroll.Core.DTO.Dept;
using HR_Payroll.Core.DTO.Leave;
using HR_Payroll.Core.DTO.Payroll;
using HR_Payroll.Core.Model.Leave;
using HR_Payroll.Core.Model.Master;
using HR_Payroll.Core.Model.Payroll;
using HR_Payroll.Core.Response;
using HR_Payroll.Web.CommonClients;
using Humanizer;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<IActionResult> LoadDepartments()
        {
            try
            {
                SetTokens();
                var result = await _apiClient.GetAsync<List<DepartmentDTO>>("Department/GetAllDepartments");
                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });
                return Json(new { status = true, result = new { data = result.data } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in LoadDepartments");
                return StatusCode(500, new { status = false, message = "An error occurred." });
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

        // Payroll views
        public IActionResult PayrollRun() => View();

        public IActionResult BankPaymentPage() => View();

        public IActionResult DeductionEntry() => View();

        public async Task<IActionResult> SalarySlip(int? employeeId, string? month)
        {
            int resolvedEmployeeId = employeeId ?? 0;
            if (resolvedEmployeeId <= 0)
            {
                var claim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;
                int.TryParse(claim, out resolvedEmployeeId);
            }

            var (payrollMonth, payrollYear) = ParseMonth(month);

            if (resolvedEmployeeId <= 0)
            {
                ViewBag.LoadError = "No employee selected.";
                return View();
            }

            try
            {
                SetTokens();
                var result = await _apiClient.GetAsync<SalarySlipDto>("Salary/GetSalarySlip",
                    new Dictionary<string, string>
                    {
                        { "employeeId",    resolvedEmployeeId.ToString() },
                        { "payrollMonth",  payrollMonth.ToString() },
                        { "payrollYear",   payrollYear.ToString() }
                    });

                if (!result.status || result.data == null)
                {
                    ViewBag.LoadError = result.message ?? "Salary slip not found.";
                    return View();
                }

                return View(result.data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SalarySlip for employee {Id}", resolvedEmployeeId);
                ViewBag.LoadError = "An error occurred while loading the salary slip.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadPayrollEmployees(string? month, int? departmentId)
        {
            if (string.IsNullOrWhiteSpace(month))
                return BadRequest(new { status = false, message = "month is required." });

            try
            {
                SetTokens();
                var qp = new Dictionary<string, string> { { "month", month } };
                if (departmentId.HasValue && departmentId.Value > 0)
                    qp["departmentId"] = departmentId.Value.ToString();

                var result = await _apiClient.GetAsync<List<PayrollRunRowDto>>("Salary/GetPayrollRunRows", qp);
                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });
                return Json(new { status = true, result = new { data = result.data } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in LoadPayrollEmployees");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CalculatePayroll([FromBody] CalculatePayrollRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Month)
                || req.EmployeeIds == null || !req.EmployeeIds.Any())
                return BadRequest(new { status = false, message = "Month and at least one employee are required." });

            try
            {
                SetTokens();
                var result = await _apiClient.PostAsync<PayrollRunResultDto>("Salary/CalculatePayroll", req);
                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });
                return Json(new { status = true, result = new { data = result.data } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in CalculatePayroll");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadBankPaymentSummary(string? month)
        {
            if (string.IsNullOrWhiteSpace(month))
                return BadRequest(new { status = false, message = "month is required." });

            try
            {
                SetTokens();
                var result = await _apiClient.GetAsync<BankPaymentSummaryDto>(
                    "Salary/GetBankPaymentSummary",
                    new Dictionary<string, string> { { "month", month } });

                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });
                return Json(new { status = true, result = new { data = result.data } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in LoadBankPaymentSummary");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkPaymentDone([FromBody] MarkPaymentDoneRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.PayrollMonth)
                || req.PayrollEmployeeIds == null || !req.PayrollEmployeeIds.Any())
                return BadRequest(new { status = false, message = "Invalid payload." });

            try
            {
                SetTokens();
                var result = await _apiClient.PostAsync<object>("Salary/MarkPaymentDone", req);
                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });
                return Json(new { status = true, message = "Payment marked as done." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in MarkPaymentDone");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadDeductionPageData(int? departmentId)
        {
            try
            {
                SetTokens();
                var qp = new Dictionary<string, string>();
                if (departmentId.HasValue && departmentId.Value > 0)
                    qp["departmentId"] = departmentId.Value.ToString();

                var result = await _apiClient.GetAsync<List<DeductionPageRowDto>>(
                    "Salary/GetDeductionPageData", qp);

                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });

                return Json(new { status = true, data = result.data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in LoadDeductionPageData");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveDeductionComponents([FromBody] SaveDeductionComponentsRequest req)
        {
            if (req == null || req.EmployeeId <= 0
                || req.Items == null || !req.Items.Any())
                return BadRequest(new { status = false, message = "Invalid payload." });

            try
            {
                SetTokens();
                var result = await _apiClient.PostAsync<object>(
                    "Salary/SaveDeductionComponents", req);

                if (!result.status)
                    return StatusCode(500, new { status = false, message = result.message });

                return Json(new { status = true, message = "Saved." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SaveDeductionComponents");
                return StatusCode(500, new { status = false, message = "An error occurred." });
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private void SetTokens()
        {
            var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;
            if (!string.IsNullOrEmpty(accessToken))
                _apiClient.SetTokens(accessToken, refreshToken ?? string.Empty);
        }

        private static (int Month, int Year) ParseMonth(string? month)
        {
            if (!string.IsNullOrWhiteSpace(month) &&
                DateTime.TryParseExact(month, "yyyy-MM", null,
                    System.Globalization.DateTimeStyles.None, out var parsed))
                return (parsed.Month, parsed.Year);

            var now = DateTime.UtcNow;
            return (now.Month, now.Year);
        }

    }
}
