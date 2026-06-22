using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Entity
{
    [Table("PayrollEmployee")]
    public class PayrollEmployee
    {
        [Key]
        public int PayrollEmployeeId { get; set; }

        public int? PayrollRunId { get; set; }
        public int? EmployeeId { get; set; }

        public decimal? Gross { get; set; }
        public decimal? Deductions { get; set; }
        public decimal? NetPay { get; set; }

        /// <summary>Draft | Calculated | Approved | Paid</summary>
        public string? Status { get; set; }

        public DateTime? ApprovedOn { get; set; }
        public DateTime? PaidOn { get; set; }
    }
}
