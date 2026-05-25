using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.TimeSheet
{
    public class SubmitTimesheetRequest
    {
        [JsonPropertyName("timesheetId")]
        public int TimesheetId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
    }
}
