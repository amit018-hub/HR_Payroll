using HR_Payroll.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Infrastructure.Interface
{
    public interface IDepartmentService
    {
        Task<List<Departments>> GetDepartmentsAsync();
    }
}
