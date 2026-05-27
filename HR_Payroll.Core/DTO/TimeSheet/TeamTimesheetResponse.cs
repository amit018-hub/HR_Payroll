using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.TimeSheet
{
    public class TeamTimesheetResponse
    {
        public List<string> Holidays { get; set; } = new(); // "yyyy-MM-dd"
        public List<TeamMemberSummary> Team { get; set; } = new();
    }

}
