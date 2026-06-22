using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Entity
{
    [Table("Payroll")]
    public class Payroll
    {
        [Key]
        public int PayrollID { get; set; }
        public int EmployeeID { get; set; }
        public int PayrollMonth { get; set; }
        public int PayrollYear { get; set; }
        public decimal? GrossEarnings { get; set; }
        public decimal? TotalDeductions { get; set; }
        public decimal? NetSalary { get; set; }
        public int? IsActive { get; set; }
        public int? IsManualEntry { get; set; }
        public string? Del_Flg { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
