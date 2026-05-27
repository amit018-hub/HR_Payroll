using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.TimeSheet
{
    public class TeamTimesheetRow
    {
        public int RowId { get; set; }
        public int SortOrder { get; set; }
        public string ProjectName { get; set; } = string.Empty;  // "Code: Name"
        public string ActivityName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        // SP returns D0-D6; mapped to hours[] in the service
        public decimal? D0 { get; set; }
        public decimal? D1 { get; set; }
        public decimal? D2 { get; set; }
        public decimal? D3 { get; set; }
        public decimal? D4 { get; set; }
        public decimal? D5 { get; set; }
        public decimal? D6 { get; set; }

        // Computed: what mgrRenderDetail() reads as r.hours
        public decimal?[] Hours => new[] { D0, D1, D2, D3, D4, D5, D6 };
    }
}
