using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Entity
{
    public class SalaryComponents
    {
        [Key]
        public int ComponentID { get; set; }
        public string? ComponentName { get; set; }
        public string? ComponentType { get; set; }
        public bool? IsEmployer { get; set; }
        public bool? IsTaxable { get; set; }
        public decimal? Percentage { get; set; }
        public string? PerOnComponentName { get; set; }
        public int? PerOnComponentID { get; set; }
        public string? Del_Flg { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
