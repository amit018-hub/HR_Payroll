using HR_Payroll.CommonCases.Utility;
using HR_Payroll.Core.DTO.Leave;
using HR_Payroll.Core.Model.Master;
using HR_Payroll.Core.Response;
using HR_Payroll.Infrastructure.Concrete;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR_Payroll.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly ILogger<MasterController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMasterServices _masterRepository;

        public MasterController(IConfiguration configuration,
            ILogger<MasterController> logger,
            IMasterServices masterRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _masterRepository = masterRepository;

        }

        [HttpPost]
        [Route("AssignShift")]
        [Authorize(Roles = "Admin,HR,Manager,Team Lead")]
        public async Task<IActionResult> ApplyLeave([FromBody] AssignEmployeeShiftRequest model)
        {
            if (model == null)
            {
                return Ok(new DataResponse<object>
                {
                    status = false,
                    message = "Invalid request",
                    data = new List<object>()
                });
            }            

            try
            {              
                var result = await _masterRepository.AssignEmployeeShiftAsync(model);

                if (result.IsSuccess && result.Entity)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = result.IsSuccess,
                        message = result.Message ?? "Shift assign successfully",
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = result.IsSuccess,
                    message = result.Message ?? "Assign shift failed",
                    data = new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during assign shift: {ex.Message}");
                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "An error occurred while processing assign shift",
                    data = new List<object>()
                });
            }
        }

    }
}
