using HR_Payroll.Core.DTO.Master;
using HR_Payroll.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Interface
{
    public interface IEmployeeShiftServices
    {
        Task<Result<IEnumerable<ShiftDto>>> GetShiftsAsync();
        Task<Result<ShiftDto>> GetShiftByCodeAsync(string shiftCode);
    }
}
