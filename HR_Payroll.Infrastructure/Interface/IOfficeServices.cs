using HR_Payroll.Core.DTO.Master;
using HR_Payroll.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Interface
{
    public interface IOfficeServices
    {
        Task<Result<IEnumerable<OfficeLocationDto>>> GetOfficeLocationsAsync();
        Task<Result<OfficeLocationDto>> GetOfficeLocationByIdAsync(int officeId);
    }
}
