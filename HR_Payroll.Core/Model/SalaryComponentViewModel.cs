using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model
{

    public class SalaryComponentViewModel
    {
        public int ComponentID { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public decimal Amount { get; set; }
        public DateTime EffectiveTo { get; set; }

    }


}
