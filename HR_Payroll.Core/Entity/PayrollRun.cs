using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Entity
{
    [Table("PayrollRun")]
    public class PayrollRun
    {
        [Key]
        public int PayrollRunId { get; set; }

        /// <summary>Stored as "yyyy-MM" varchar in DB</summary>
        public string? PayrollMonth { get; set; }

        /// <summary>Draft | Calculated | Approved | Paid</summary>
        public string? Status { get; set; }

        public DateTime? CreatedOn { get; set; }

        /// <summary>FK to Users.UserID (int), not a name string</summary>
        public int? CreatedBy { get; set; }
    }
}
