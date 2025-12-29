using HR_Payroll.Core.DTO.Dept;
using HR_Payroll.Core.DTO.Master;
using HR_Payroll.Core.Model.DataTable;
using HR_Payroll.Core.Model.Master;
using HR_Payroll.Core.Services;
using HR_Payroll.Web.CommonClients;
using Microsoft.AspNetCore.Mvc;

namespace HR_Payroll.Web.Controllers
{
    public class AssignController : Controller
    {
        private readonly CommonAPI_Client _apiClient;
        private readonly ILogger<AssignController> _logger;
        private readonly IConfiguration _configuration;

        public AssignController(CommonAPI_Client apiClient,
            ILogger<AssignController> logger,
            IConfiguration configuration)
        {
            _apiClient = apiClient;
            _logger = logger;
            _configuration = configuration;
        }
        public string _apiBaseUrl => _configuration.GetValue<string>("ApiBaseUrl");

        [HttpGet]
        public async Task<IActionResult> GetOfficeLocations()
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

                var result = await _apiClient.GetAsync<List<OfficeLocationDto>>("OfficeLocation/GetAllOfficeLocation");

                if (!result.status)
                {
                    _logger.LogWarning("Failed to retrieve office locations: {Message}", result.message);
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Office locations retrieved successfully.",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetOfficeLocations");
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while retrieving office locations."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeShifts()
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

                var result = await _apiClient.GetAsync<List<ShiftDto>>("EmployeeShift/GetAllEmployeeShift");

                if (!result.status)
                {
                    _logger.LogWarning("Failed to retrieve employee shift: {Message}", result.message);
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Employee shift retrieved successfully.",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetEmployeeShifts");
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while retrieving employee shift."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
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

                var result = await _apiClient.GetAsync<List<DepartmentDTO>>("Department/GetAllDepartments");

                if (!result.status)
                {
                    _logger.LogWarning("Failed to retrieve departments: {Message}", result.message);
                    return StatusCode(500, new
                    {
                        status = false,
                        message = result.message
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Departments retrieved successfully.",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetDepartments");
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while retrieving departments."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubDepartments(int departmentId)
        {
            var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { status = false, message = "Missing tokens." });
            }

            _apiClient.SetTokens(accessToken, refreshToken);

            var result = await _apiClient.GetAsync<List<SubDepartmentDTO>>(
                "SubDepartment/GetAllSubDepartments",
                new Dictionary<string, string> { { "departmentId", departmentId.ToString() } }
            );

            if (!result.status)
            {
                return StatusCode(500, new { status = false, message = result.message });
            }

            return Json(new
            {
                status = true,
                message = "Sub-departments loaded.",
                data = result.data
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeBySubDept(int subDepartmentId)
        {
            var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { status = false, message = "Missing tokens." });
            }

            _apiClient.SetTokens(accessToken, refreshToken);

            var result = await _apiClient.GetAsync<List<BranchWiseUserModel>>(
                "SubDepartment/GetEmployeeBySubDepartment",
                new Dictionary<string, string> { { "subDepartmentId", subDepartmentId.ToString() } }
            );

            if (!result.status)
            {
                return StatusCode(500, new { status = false, message = result.message });
            }

            return Json(new
            {
                status = true,
                message = "employee loaded.",
                data = result.data
            });
        }

        public IActionResult AssignEmployee() => View();

        [HttpPost]
        public async Task<IActionResult> AssignHierarchy(DepartmentAssignDTO dto)
        {
            try
            {
                // Capture IP address
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                dto.CreatedBy ??= ipAddress ?? "Unknown";

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
                var result = await _apiClient.PostAsync<DepartmentAssignResult>(
                    "Department/DeptAssignHierarchy", // ✅ correct endpoint
                    dto
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
                    message = "Department hierarchy assigned successfully.",
                    data = result.data
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

        [HttpGet]
        public async Task<IActionResult> GetAssignHierarchyList(PaginationDataRequestModel model)
        {
            // 🔐 Extract tokens from claims
            var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { status = false, message = "Missing authentication tokens." });
            }

            // ✅ Set tokens to API client
            _apiClient.SetTokens(accessToken, refreshToken);

            // ✅ Prepare query parameters
            var queryParams = new Dictionary<string, string>
            {               
                { "Start", model.Start.ToString() },
                { "Length", model.Length.ToString() },
                { "Search", model.Search ?? null },
                { "SortColumn", model.SortColumn ?? "EmployeeName" },
                { "SortDirection", model.SortDirection ?? "ASC" }
            };

            // ✅ Call the API
            var result = await _apiClient.GetAsync<List<AssignEmployeeListModel>>("Department/GetHierarchyList", queryParams);

            if (!result.status)
            {
                return StatusCode(500, new { status = false, message = result.message });
            }

            // ✅ Return paginated response (ideal for DataTables)
            return Json(new
            {
                status = true,
                message = "Hierarchy loaded successfully.",
                data = result.data,
                recordsTotal = result.data[0].TotalRecords,
                recordsFiltered = result.data?.Count ?? 0
            });
        }

        public IActionResult OrgFlowChart() => PartialView("FlowChart/_pOrgEmployeeChart");

        public IActionResult AssignManager() => View();
        public IActionResult AssignTeamLeader() => View();

        public IActionResult AssignEmployeeShift() => View();

        [HttpPost]
        public async Task<IActionResult> AssignEmployeeShift(List<AssignEmployeeShiftRequest> requests)
        {
            try
            {
                if (requests == null || !requests.Any())
                {
                    return Json(new { 
                        status = false, 
                        message = "Invalid data" 
                    });
                }
                
                // Get tokens from claims
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                var refreshToken = User.Claims.FirstOrDefault(c => c.Type == "refresh_token")?.Value;

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { status = false, message = "Missing tokens." });
                }

                // Set tokens for HttpClient
                _apiClient.SetTokens(accessToken, refreshToken);

                var failed = new List<string>();

                foreach (var request in requests)
                {
                    try
                    {
                        request.CreatedBy ??= HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                        var result = await _apiClient.PostAsync<object>(
                            "Master/AssignShift",
                            request
                        );

                        if (!result.status)
                        {
                            failed.Add($"Emp:{request.EmployeeId}, Office:{request.OfficeId}");
                        }
                    }
                    catch
                    {
                        failed.Add($"Emp:{request.EmployeeId}, Office:{request.OfficeId}");
                    }
                }

                if (failed.Any())
                {
                    return Json(new
                    {
                        status = false,
                        message = "assign shift failed",
                        failedRecords = failed
                    });
                }

                return Json(new
                {
                    status = true,
                    message = "Shifts assigned successfully."
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

    }
}
