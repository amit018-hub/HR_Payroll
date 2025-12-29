using HR_Payroll.API.JWTExtension;
using HR_Payroll.Infrastructure.Concrete;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.AspNetCore.Identity;

namespace HR_Payroll.API
{
    public static class ServiceExtension
    {
        public static void ConfigureDIServices(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();       
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<JWTServiceExtension>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IMasterServices, MasterServices>();
            services.AddScoped<IDepartmentServices, DepartmentServices>();
            services.AddScoped<ISubDepartmentServices, SubDepartmentServices>();
            services.AddScoped<ILeaveService, LeaveService>();
            services.AddScoped<IEmployeeService, EmployeeService>();     
            services.AddScoped<ISalaryService, SalaryService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IEmployeeShiftServices, EmployeeShiftServices>();
            services.AddScoped<IOfficeServices, OfficeServices>();
        }
    }
}
