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
    public class DepartmentService: IDepartmentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(AppDbContext context, ILogger<DepartmentService> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<Departments>> GetDepartmentsAsync()
        {
            try
            {
                var data = await _context.Departments
                    .Where(d => d.Del_Flg != "Y") 
                    .ToListAsync();
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching departments");
                return new List<Departments>();
            }
        }


    }
}
