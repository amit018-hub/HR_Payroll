using Dapper;
using HR_Payroll.Core.DTO.Master;
using HR_Payroll.Core.Services;
using HR_Payroll.Infrastructure.Data;
using HR_Payroll.Infrastructure.Interface;
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
    public class EmployeeShiftServices : IEmployeeShiftServices
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmployeeShiftServices> _logger;

        public EmployeeShiftServices(AppDbContext context, ILogger<EmployeeShiftServices> logger)
        {
            _logger = logger;
            _context = context;
        }

        #region Dapper Helpers (same as your base)

        private async Task<Result<T>> QuerySingleWithDapperAsync<T>(string sql, DynamicParameters parameters = null)
        {
            using var connection = _context.Database.GetDbConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                var result = await connection.QueryFirstOrDefaultAsync<T>(
                    sql,
                    parameters,
                    commandType: CommandType.Text
                );

                if (result == null)
                    return Result<T>.Failure($"{typeof(T).Name} not found.");

                return Result<T>.Success(result, $"{typeof(T).Name} retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Result<T>.Failure($"Error retrieving {typeof(T).Name}: {ex.Message}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        private async Task<Result<IEnumerable<T>>> QueryWithDapperAsync<T>(string sql, DynamicParameters parameters = null, CommandType commandType = CommandType.Text)
        {
            using var connection = _context.Database.GetDbConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                var result = await connection.QueryAsync<T>(sql, parameters, commandType: commandType);

                if (result == null || !result.Any())
                    return Result<IEnumerable<T>>.Failure($"No {typeof(T).Name}s found.");

                return Result<IEnumerable<T>>.Success(
                    result.ToList(),
                    $"{typeof(T).Name}s retrieved successfully."
                );
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<T>>.Failure($"Error retrieving {typeof(T).Name}s: {ex.Message}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        #endregion

        public async Task<Result<ShiftDto>> GetShiftByCodeAsync(string shiftCode)
        {
            const string sql = @"
            SELECT 
                ShiftCode,
                ShiftName,
                StartTime,
                EndTime,
                IsActive
                FROM [dbo].[ShiftMaster]
                WHERE ShiftCode = @ShiftCode
                  AND IsActive = 1;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@ShiftCode", shiftCode);

            return await QuerySingleWithDapperAsync<ShiftDto>(sql, parameters);
        }

        public async Task<Result<IEnumerable<ShiftDto>>> GetShiftsAsync()
        {
            const string sql = @"
            SELECT 
                ShiftCode,
                ShiftName,
                StartTime,
                EndTime,
                IsActive
                FROM [dbo].[ShiftMaster]
                WHERE IsActive = 1
                ORDER BY StartTime;
            ";

            return await QueryWithDapperAsync<ShiftDto>(sql);
        }
    }
}
