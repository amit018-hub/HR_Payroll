using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Entity
{
    [Table("PaymentBatch")]
    public class PaymentBatch
    {
        [Key]
        public int PaymentBatchId { get; set; }
        public int? PayrollRunId { get; set; }
        public string? FilePath { get; set; }
        public string? Status { get; set; }
        public DateTime? GeneratedOn { get; set; }
        public int? GeneratedBy { get; set; }
    }
}
