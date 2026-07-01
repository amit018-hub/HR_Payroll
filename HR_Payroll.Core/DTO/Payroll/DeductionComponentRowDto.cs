using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.Payroll
{
    public class DeductionComponentRowDto
    {
        public int ComponentId { get; set; }
        public string? ComponentName { get; set; }
        public string? ComponentType { get; set; }
        public decimal? Percentage { get; set; }
        public string? PerOnComponentName { get; set; }
        public bool IsMandatory { get; set; }

        public decimal? CurrentAmount { get; set; }
        public bool IsConfigured { get; set; }
    }
}
