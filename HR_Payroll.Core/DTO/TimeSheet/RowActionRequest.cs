using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.TimeSheet
{
    public class RowActionRequest
    {
        public int RowId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
    }
}
