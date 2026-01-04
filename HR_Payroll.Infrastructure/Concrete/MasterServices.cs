using Dapper;
using HR_Payroll.Core.DTO.Dept;
using HR_Payroll.Core.Model.Master;
using HR_Payroll.Core.Services;
using HR_Payroll.Infrastructure.Data;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Concrete
{
    public class MasterServices : IMasterServices
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MasterServices> _logger;

        public MasterServices(AppDbContext context, ILogger<MasterServices> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result<bool>> AssignEmployeeShiftAsync(AssignEmployeeShiftRequest request)
        {
            try
            {
                using var conn = _context.Database.GetDbConnection();

                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeID", request.EmployeeId);
                parameters.Add("@ShiftCode", request.ShiftCode);
                parameters.Add("@OfficeID", request.OfficeId);
                parameters.Add("@EffectiveFrom", request.FromDate);
                parameters.Add("@EffectiveTo", request.ToDate);
                parameters.Add("@CreatedBy", request.CreatedBy);

                var result = await conn.QueryFirstOrDefaultAsync<SpAssignShiftResult>(
                    "sp_AssignEmployeeShift",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return new Result<bool>
                {
                    IsSuccess = result?.Success == 1,
                    Message = result?.Message ?? "Shift assignment failed",
                    Entity = result?.Success == 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while assigning employee shift");

                return new Result<bool>
                {
                    IsSuccess = false,
                    Message = "An error occurred while assigning shift",
                    Entity = false
                };
            }
        }

    }
}
