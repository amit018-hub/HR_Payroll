using HR_Payroll.Core.Model;
using HR_Payroll.Core.Model.Dashboard;
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
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<DashboardViewModel>> GetAdminDashboardData()
        {
            var dashboardData = new DashboardViewModel
            {
                AttendanceChart = new List<AttendanceChartData>(),
                LeaveTypeSummary = new List<LeaveTypeSummary>(),
                RecentAttendance = new List<RecentAttendanceData>()
            };

            try
            {
                using (var conn = _context.Database.GetDbConnection())
                {
                    if (conn.State != ConnectionState.Open)
                        await conn.OpenAsync();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_Admin_DashboardData";
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            // 1️⃣ Total Employees
                            if (await reader.ReadAsync())
                                dashboardData.TotalEmployees = reader.GetInt32(0);

                            // 2️⃣ Present Today
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                                dashboardData.PresentToday = reader.GetInt32(0);

                            // 3️⃣ Absent Today
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                                dashboardData.AbsentToday = reader.GetInt32(0);

                            // 4️⃣ Leave Today
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                                dashboardData.LeaveToday = reader.GetInt32(0);

                            // 5️⃣ WFH Today
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                                dashboardData.WFHCount = reader.GetInt32(0);

                            // 6️⃣ Attendance Chart
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dashboardData.AttendanceChart.Add(new AttendanceChartData
                                    {
                                        Date = Convert.ToDateTime(reader["AttDate"]).ToString("yyyy-MM-dd"),
                                        PresentCount = Convert.ToInt32(reader["PresentCount"]),
                                        AbsentCount = Convert.ToInt32(reader["AbsentCount"]),
                                        WFHCount = Convert.ToInt32(reader["WFHCount"])
                                    });
                                }
                            }

                            // 7️⃣ Leave Type Summary
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dashboardData.LeaveTypeSummary.Add(new LeaveTypeSummary
                                    {
                                        Type = reader["LeaveType"].ToString(),
                                        Count = Convert.ToInt32(reader["LeaveCount"])
                                    });
                                }
                            }

                            // 8️⃣ Recent Attendance
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dashboardData.RecentAttendance.Add(new RecentAttendanceData
                                    {
                                        EmployeeName = reader["EmployeeName"].ToString(),
                                        Date = Convert.ToDateTime(reader["Date"]).ToString("dd MMM yyyy"),
                                        CheckIn = reader["CheckIn"]?.ToString() ?? "-",
                                        CheckOut = reader["CheckOut"]?.ToString() ?? "-",
                                        Status = reader["Status"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }

                // 9️⃣ Percentage Calculations
                if (dashboardData.TotalEmployees > 0)
                {
                    dashboardData.TodayPresentPercentage =
                        Math.Round((decimal)dashboardData.PresentToday / dashboardData.TotalEmployees * 100, 2);

                    dashboardData.LeavePercentage =
                        Math.Round((decimal)dashboardData.LeaveToday / dashboardData.TotalEmployees * 100, 2);

                    dashboardData.WFHPercentage =
                        Math.Round((decimal)dashboardData.WFHCount / dashboardData.TotalEmployees * 100, 2);
                }

                return new Result<DashboardViewModel>
                {
                    Entity = dashboardData,
                    IsSuccess = true,
                    Message = "Dashboard data loaded"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard loading failed");

                return new Result<DashboardViewModel>
                {
                    Entity = new DashboardViewModel(),
                    IsSuccess = false,
                    Message = "Failed: " + ex.Message
                };
            }
        }

        public async Task<Result<EmployeeDashboardViewModel>> GetEmployeeDashboardData(int employeeId)
        {
            var dashboardData = new EmployeeDashboardViewModel
            {
                AttendanceChart = new List<EmployeeAttendanceChartData>(),
                RecentAttendance = new List<EmployeeRecentAttendanceData>()
            };

            try
            {
                using (var conn = _context.Database.GetDbConnection())
                {
                    if (conn.State != ConnectionState.Open)
                        await conn.OpenAsync();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_Employee_DashboardData";
                        cmd.CommandType = CommandType.StoredProcedure;

                        var param = cmd.CreateParameter();
                        param.ParameterName = "@EmployeeID";
                        param.Value = employeeId;
                        cmd.Parameters.Add(param);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            /* =================================================
                               1️⃣ Employee Basic Info (Optional – skip)
                               ================================================= */
                            await reader.NextResultAsync();

                            /* =================================================
                               2️⃣ Today Attendance Status
                               ================================================= */
                            if (await reader.ReadAsync())
                                dashboardData.TodayStatus = reader["TodayStatus"]?.ToString() ?? "Absent";

                            /* =================================================
                               3️⃣ Today Working Hours
                               ================================================= */
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                                dashboardData.TodayWorkingHours = reader[0]?.ToString() ?? "0h 00m";

                            /* =================================================
                               4️⃣ Monthly Working Hours
                               ================================================= */
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                                dashboardData.MonthlyWorkingHours = reader[0]?.ToString() ?? "0h";

                            /* =================================================
                               5️⃣ Leave Taken (Year)
                               ================================================= */
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                                dashboardData.LeaveTaken = Convert.ToInt32(reader["LeaveTaken"]);

                            /* =================================================
                               6️⃣ Attendance Chart (Last 30 Days)
                               ================================================= */
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dashboardData.AttendanceChart.Add(new EmployeeAttendanceChartData
                                    {
                                        AttDate = Convert.ToDateTime(reader["AttDate"]).ToString("yyyy-MM-dd"),
                                        IsPresent = Convert.ToInt32(reader["IsPresent"])
                                    });
                                }
                            }

                            /* =================================================
                               7️⃣ Leave Type Summary (Not used in UI – skip)
                               ================================================= */
                            await reader.NextResultAsync();

                            /* =================================================
                               8️⃣ Recent Attendance
                               ================================================= */
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dashboardData.RecentAttendance.Add(new EmployeeRecentAttendanceData
                                    {
                                        Date = Convert.ToDateTime(reader["Date"]).ToString("dd MMM yyyy"),
                                        CheckIn = reader["CheckIn"]?.ToString() ?? "-",
                                        CheckOut = reader["CheckOut"]?.ToString() ?? "-",
                                        Status = reader["Status"]?.ToString() ?? "N/A"
                                    });
                                }
                            }
                        }
                    }
                }

                return new Result<EmployeeDashboardViewModel>
                {
                    Entity = dashboardData,
                    IsSuccess = true,
                    Message = "Employee dashboard data loaded"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Employee dashboard loading failed");

                return new Result<EmployeeDashboardViewModel>
                {
                    Entity = new EmployeeDashboardViewModel(),
                    IsSuccess = false,
                    Message = "Failed: " + ex.Message
                };
            }
        }

    }

}
