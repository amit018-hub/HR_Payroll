using HR_Payroll.Core.Entity;
using HR_Payroll.Infrastructure.Data;
using HR_Payroll.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Concrete
{
    public class SubDepartmentService:ISubDepartmentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SubDepartmentService> _logger;

        public SubDepartmentService(AppDbContext context, ILogger<SubDepartmentService> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<SubDepartments>> GetSubDepartmentsAsync(int deptid)
        {
            try
            {
                return await _context.SubDepartments
                    .Where(d => d.Del_Flg != "Y" && d.DepartmentId == deptid)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching departments");
                return new List<SubDepartments>();
            }
        }
    }
}
