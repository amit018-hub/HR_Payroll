using HR_Payroll.Core.Model.Dashboard;
using HR_Payroll.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Interface
{
    public interface IDashboardService
    {
        Task<Result<DashboardViewModel>> GetAdminDashboardData();
        Task<Result<EmployeeDashboardViewModel>> GetEmployeeDashboardData(int employeeId);
    }
}
