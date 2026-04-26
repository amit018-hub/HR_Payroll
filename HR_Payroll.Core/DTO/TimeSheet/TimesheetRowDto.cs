using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.TimeSheet
{
    public class TimesheetRowDto
    {
        public int RowId { get; set; }
        public int SortOrder { get; set; }
        public int ProjectId { get; set; }
        public string ProjectDisplay { get; set; } = string.Empty;
        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ShiftId { get; set; } = string.Empty;  // NVARCHAR ShiftCode
        public string ShiftName { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        // D0…D6 mapped from SP pivot columns
        public decimal? D0 { get; set; }
        public decimal? D1 { get; set; }
        public decimal? D2 { get; set; }
        public decimal? D3 { get; set; }
        public decimal? D4 { get; set; }
        public decimal? D5 { get; set; }
        public decimal? D6 { get; set; }

        // Computed: converts D0-D6 into the array the JS expects
        public decimal?[] Hours => new[] { D0, D1, D2, D3, D4, D5, D6 };
    }
}
