using HR_Payroll.Core.Response;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR_Payroll.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficeLocationController : ControllerBase
    {
        private readonly ILogger<OfficeLocationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOfficeServices _officeRepository;

        public OfficeLocationController(IConfiguration configuration,
            ILogger<OfficeLocationController> logger,
            IOfficeServices officeRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _officeRepository = officeRepository;

        }

        [HttpGet]
        [Route("GetAllOfficeLocation")]
        public async Task<ActionResult> GetAllOfficeLocation()
        {
            try
            {
                var result = await _officeRepository.GetOfficeLocationsAsync();

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message ?? "Failed to retrieve office location list",
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message ?? "Office location list retrieved successfully",
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving office location list");
                return StatusCode(500, new DataResponse<object>
                {
                    status = false,
                    message = "An error occurred while processing your request.",
                    data = new List<object>()
                });
            }
        }

        [HttpGet]
        [Route("GetOfficeById")]
        public async Task<ActionResult> GetOfficeById([FromQuery] int officeId)
        {
            try
            {
                var result = await _officeRepository.GetOfficeLocationByIdAsync(officeId);

                if (!result.IsSuccess)
                {
                    return Ok(new DataResponse<object>
                    {
                        status = false,
                        message = result.Message ?? "Failed to retrieve office location",
                        data = new List<object>()
                    });
                }

                return Ok(new DataResponse<object>
                {
                    status = true,
                    message = result.Message ?? "Office location retrieved successfully",
                    data = result.Entity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving office location");
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
