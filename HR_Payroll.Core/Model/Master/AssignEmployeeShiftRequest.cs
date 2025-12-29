using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.Master
{
    public class AssignEmployeeShiftRequest
    {
        public int EmployeeId { get; set; }
        public int OfficeId { get; set; }
        public string? ShiftCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}
