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

                const string sql = @"
                    INSERT INTO EmployeeShiftAssignment
                    (EmployeeID, OfficeID, ShiftCode, EffectiveFrom, EffectiveTo, IsActive, Del_Flg, CreatedBy, CreatedDate)
                    VALUES
                    (@EmployeeId, @OfficeId, @ShiftCode, @FromDate, @ToDate, 1, 'N', @CreatedBy, GETDATE())
                ";

                var rows = await conn.ExecuteAsync(sql, request);

                return new Result<bool>
                {
                    IsSuccess = rows > 0,
                    Message = rows > 0 ? "Shift assign successfully" : "Failed to shift assign",
                    Entity = rows > 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in assign shift");
                return new Result<bool>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Entity = false
                };
            }
        }
    }
}
