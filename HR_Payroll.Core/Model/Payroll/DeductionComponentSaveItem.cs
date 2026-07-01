using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.Payroll
{
    public class DeductionComponentSaveItem
    {
        public int ComponentId { get; set; }
        public decimal Amount { get; set; }
    }
}
