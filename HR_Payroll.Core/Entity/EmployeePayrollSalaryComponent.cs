using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Entity
{
    public class EmployeePayrollSalaryComponent
    {
        [Key]
        public int SalaryComponentID { get; set; }
        public int EmployeeID { get; set; }
        public int ComponentID { get; set; }
        public decimal Amount { get; set; }
        public int PayrollMonth { get; set; }
        public int PayrollYear { get; set; }
        public int? IsActive { get; set; }
        public string? Del_Flg { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public int? PayrollID { get; set; }
    }
}
