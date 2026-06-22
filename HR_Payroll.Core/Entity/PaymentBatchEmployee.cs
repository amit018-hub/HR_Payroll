using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Entity
{
    [Table("PaymentBatchEmployee")]
    public class PaymentBatchEmployee
    {
        [Key]
        public int PaymentBatchEmployeeId { get; set; }
        public int? PaymentBatchId { get; set; }
        public int? EmployeeId { get; set; }
        public decimal? NetPay { get; set; }
        public string? Status { get; set; }
    }
}
