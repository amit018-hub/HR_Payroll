using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.TimeSheet
{
    public class TeamTimesheetDetail
    {
        public TeamTimesheetHeader Header { get; set; } = new();
        public List<TeamTimesheetRow> Rows { get; set; } = new();
    }
}
