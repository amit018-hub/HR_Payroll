using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.TimeSheet
{
    public class RowSaveItem
    {
        public int RowId { get; set; }        // 0 = new row
        public int ProjectId { get; set; }
        public int ActivityId { get; set; }
        public int CategoryId { get; set; }
        public string ShiftId { get; set; } = string.Empty;   // ShiftMaster.ShiftCode (NVARCHAR)
        public string Remarks { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public decimal?[] Hours { get; set; } = new decimal?[7];
    }
}
