using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.TimeSheet
{
    public class TimesheetAuditDto
    {
        public int LogId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public DateTime ActionOn { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string ActionByCode { get; set; } = string.Empty;
        public string ActionByName { get; set; } = string.Empty;
    }
}
