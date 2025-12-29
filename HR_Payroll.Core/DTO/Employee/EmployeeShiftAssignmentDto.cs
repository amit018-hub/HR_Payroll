using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.Employee
{
    public class EmployeeShiftAssignmentDto
    {
        public int EmployeeID { get; set; }
        public string? ShiftCode { get; set; }
        public int OfficeID { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

    }
}
