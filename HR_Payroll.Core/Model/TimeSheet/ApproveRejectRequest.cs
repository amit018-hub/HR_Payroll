using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.TimeSheet
{
    public class ApproveRejectRequest
    {
        public int TimesheetId { get; set; }
        public string Action { get; set; } = string.Empty;   // "APPROVED" | "REJECTED"
        public string Remarks { get; set; } = string.Empty;
        public string ApproverCode { get; set; } = string.Empty;
    }
}
