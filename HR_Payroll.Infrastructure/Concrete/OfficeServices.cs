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
    public class OfficeServices : IOfficeServices
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OfficeServices> _logger;

        public OfficeServices(AppDbContext context, ILogger<OfficeServices> logger)
        {
            _logger = logger;
            _context = context;
        }

        #region Dapper Helpers (Already in your base repo – pasted here for clarity)

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

        public async Task<Result<OfficeLocationDto>> GetOfficeLocationByIdAsync(int officeId)
        {
            const string sql = @"
            SELECT 
                OfficeID,
                OfficeName,
                Address,
                Latitude,
                Longitude,
                GeoFenceRadius,
                IsActive
                FROM [dbo].[OfficeLocation]
                WHERE OfficeID = @OfficeID
                  AND Del_Flg = 'N';
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@OfficeID", officeId);

            return await QuerySingleWithDapperAsync<OfficeLocationDto>(sql, parameters);
        }

        public async Task<Result<IEnumerable<OfficeLocationDto>>> GetOfficeLocationsAsync()
        {
            const string sql = @"
            SELECT 
                OfficeID,
                OfficeName,
                Address,
                Latitude,
                Longitude,
                GeoFenceRadius,
                IsActive
                FROM [dbo].[OfficeLocation]
                WHERE Del_Flg = 'N'
                ORDER BY OfficeName;
            ";

            return await QueryWithDapperAsync<OfficeLocationDto>(sql);
        }
    }
}
