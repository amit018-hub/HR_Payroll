using HR_Payroll.Core.Model;
using HR_Payroll.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HR_Payroll.Web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;

        public EmployeeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiBaseUrl = _configuration["ApiBaseUrl"];
        }
        public IActionResult EmployeeMaster()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetDepartments()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{_apiBaseUrl}Department/GetAllDepartments");
                if (response.IsSuccessStatusCode)
                {
                    var departments = await response.Content.ReadFromJsonAsync<object>();
                    return Json(new { status = true, result = departments });
                }
                return Json(new { status = false, result = new object[0] });
            }
            catch(Exception ex)
            {
                throw;
            }
           
        }

        [HttpGet]
        public async Task<JsonResult> GetSubDepartments(int deptid)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiBaseUrl}SubDepartment/GetAllSubDepartments?deptid={deptid}");
            if (response.IsSuccessStatusCode)
            {
                var subDepartments = await response.Content.ReadFromJsonAsync<object>();
                return Json(new { status = true, result = subDepartments });
            }
            return Json(new { status = false, result = new object[0] });
        }

        [HttpGet]
        public async Task<JsonResult> GetSalaryComponents()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{_apiBaseUrl}Salary/GetSalaryComponents");
                if (response.IsSuccessStatusCode)
                {
                    var components = await response.Content.ReadFromJsonAsync<object>();
                    return Json(new { status = true, result = components });
                }
                return Json(new { status = false, result = new object[0] });
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        public async Task<JsonResult> SaveBasicInfo([FromForm] EmployeeBasicInfoViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                using var multipart = new MultipartFormDataContent();

                multipart.Add(new StringContent(model.EmployeeId?.ToString() ?? ""), "EmployeeId");
                multipart.Add(new StringContent(model.EmployeeCode ?? ""), "EmployeeCode");
                multipart.Add(new StringContent(model.FirstName ?? ""), "FirstName");
                multipart.Add(new StringContent(model.LastName ?? ""), "LastName");
                multipart.Add(new StringContent(model.DepartmentId?.ToString() ?? ""), "DepartmentId");
                multipart.Add(new StringContent(model.SubDepartmentId?.ToString() ?? ""), "SubDepartmentId");
                multipart.Add(new StringContent(model.State ?? ""), "State");
                multipart.Add(new StringContent(model.JoiningDate?.ToString("o") ?? ""), "JoiningDate");
                multipart.Add(new StringContent(model.ReportingTo ?? ""), "ReportingTo");
                multipart.Add(new StringContent(model.SourceOfHire ?? ""), "SourceOfHire");
                multipart.Add(new StringContent(model.Interviewer ?? ""), "Interviewer");
                multipart.Add(new StringContent(model.AttendanceRules ?? ""), "AttendanceRules");
                multipart.Add(new StringContent(model.EmploymentStatus?.ToString() ?? ""), "EmploymentStatus");
                multipart.Add(new StringContent(model.MaritalStatus ?? ""), "MaritalStatus");
                multipart.Add(new StringContent(model.AadharNo ?? ""), "AadharNo");
                multipart.Add(new StringContent(model.PANNo ?? ""), "PANNo");
                multipart.Add(new StringContent(model.PFNo ?? ""), "PFNo");
                multipart.Add(new StringContent(model.UANNo ?? ""), "UANNo");
                multipart.Add(new StringContent(model.ESINo ?? ""), "ESINo");
                multipart.Add(new StringContent(model.NoticePeriod?.ToString() ?? ""), "NoticePeriod");

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var stream = model.ProfilePicture.OpenReadStream();
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.ProfilePicture.ContentType ?? "application/octet-stream");

                    multipart.Add(streamContent, "ProfilePicture", model.ProfilePicture.FileName);
                }

                var response = await client.PostAsync($"{_apiBaseUrl}Employee/SaveBasicInfo", multipart);

                var content = response.IsSuccessStatusCode
                    ? await response.Content.ReadFromJsonAsync<object>()
                    : new { status = false, message = "API save failed" };

                return Json(new { status = response.IsSuccessStatusCode, result = content });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SavePayrollInfo([FromBody] EmployeePayrollViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}Employee/SavePayrollInfo", model);
                var content = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<object>() : new { status = false };
                return Json(new { status = response.IsSuccessStatusCode, result = content });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveBankDetails([FromBody] EmployeeBankViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}Employee/SaveBankDetails", model);
                var content = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<object>() : new { status = false };
                return Json(new { status = response.IsSuccessStatusCode, result = content });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveSalaryBreakup([FromBody] EmployeeSalaryBreakupViewModel model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}Employee/SaveSalaryBreakup", model);
                var content = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<object>() : new { status = false };
                return Json(new { status = response.IsSuccessStatusCode, result = content });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> SaveAllEmployeeData([FromForm] EmployeeBasicInfoViewModel basic, [FromForm] EmployeePayrollViewModel payroll,
               [FromForm] EmployeeBankViewModel bank, [FromForm] string SalaryComponents, [FromForm] IFormFile? ProfilePicture)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                using var multipart = new MultipartFormDataContent();

                // Append Basic Properties (flat)
                foreach (var prop in basic.GetType().GetProperties())
                {
                    var value = prop.GetValue(basic)?.ToString() ?? "";
                    multipart.Add(new StringContent(value), prop.Name);
                }

                // Append Payroll Properties (flat)
                foreach (var prop in payroll.GetType().GetProperties())
                {
                    var value = prop.GetValue(payroll)?.ToString() ?? "";
                    multipart.Add(new StringContent(value), prop.Name);
                }

                // Append Bank Properties (flat)
                foreach (var prop in bank.GetType().GetProperties())
                {
                    var value = prop.GetValue(bank)?.ToString() ?? "";
                    multipart.Add(new StringContent(value), prop.Name);
                }

                // Salary Components JSON
                multipart.Add(new StringContent(SalaryComponents, Encoding.UTF8, "application/json"), "SalaryComponents");

                // Profile Picture
                if (ProfilePicture != null && ProfilePicture.Length > 0)
                {
                    var streamContent = new StreamContent(ProfilePicture.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(ProfilePicture.ContentType);
                    multipart.Add(streamContent, "ProfilePicture", ProfilePicture.FileName);
                }

                // Send to API
                var response = await client.PostAsync($"{_apiBaseUrl}Employee/SaveAllEmployeeData", multipart);
                var content = await response.Content.ReadAsStringAsync();

                return Json(new { status = response.IsSuccessStatusCode, response = content });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, error = ex.Message });
            }
        }


    }

}

