using HR_Payroll.Core.DTO.TimeSheet;
using HR_Payroll.Core.Model.TimeSheet;
using HR_Payroll.Core.Response;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;

namespace HR_Payroll.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimesheetController : ControllerBase
    {
        private readonly ILogger<TimesheetController> _logger;
        private readonly ITimeSheetServices _timesheetservice;

        public TimesheetController(
            ITimeSheetServices timesheetservice,
            ILogger<TimesheetController> logger)
        {
            _logger = logger;
            _timesheetservice = timesheetservice;
        }

        private string GetCurrentEmployeeCode()
        {
            return HttpContext?.User.FindFirst("EmpCode")?.Value ?? "";
        }

        [HttpGet("GetTimesheet")]
        [Authorize(Roles = "Admin,Employee,Manager,Team Lead")]
        public async Task<IActionResult> Get([FromQuery] int weekOffset = 0)
        {
            try
            {
                var result = await _timesheetservice
                    .GetViewModelAsync(GetCurrentEmployeeCode(), weekOffset);

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message,
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message,
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timesheet");

                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "Error retrieving timesheet",
                    data = new List<object>()
                });
            }
        }

        [HttpPost("save")]
        [Authorize(Roles = "Employee,HR,Manager,Team Lead")]
        public async Task<IActionResult> Save([FromBody] SaveTimesheetRequest req)
        {
            try
            {
                req.EmployeeCode = GetCurrentEmployeeCode();

                if (!ModelState.IsValid)
                {
                    return BadRequest(new DataResponse<object>
                    {
                        status = false,
                        message = string.Join("; ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)),
                        data = new List<object>()
                    });
                }

                var result = await _timesheetservice.SaveAsync(req);

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message,
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message,
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving timesheet");

                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "Error saving timesheet",
                    data = new List<object>()
                });
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitTimesheetRequest req)
        {
            try
            {
                req.EmployeeCode = GetCurrentEmployeeCode();

                var result = await _timesheetservice.SubmitAsync(req);

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message,
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message,
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting timesheet");

                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "Error submitting timesheet",
                    data = new List<object>()
                });
            }
        }

        [HttpDelete("row/{rowId:int}")]
        public async Task<IActionResult> DeleteRow(int rowId)
        {
            try
            {
                var result = await _timesheetservice
                    .DeleteRowAsync(rowId, GetCurrentEmployeeCode());

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message,
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message,
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting row");

                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "Error deleting row",
                    data = new List<object>()
                });
            }
        }

        [HttpPatch("row/{rowId:int}/reset")]
        public async Task<IActionResult> ResetRow(int rowId)
        {
            try
            {
                var result = await _timesheetservice
                    .ResetRowAsync(rowId, GetCurrentEmployeeCode());

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message,
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message,
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting row");

                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "Error resetting row",
                    data = new List<object>()
                });
            }
        }

        [HttpGet("{timesheetId:int}/history")]
        public async Task<IActionResult> History(int timesheetId)
        {
            try
            {
                var result = await _timesheetservice.GetHistoryAsync(timesheetId);

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message,
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message,
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving history");

                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "Error retrieving history",
                    data = new List<object>()
                });
            }
        }
    }
}
