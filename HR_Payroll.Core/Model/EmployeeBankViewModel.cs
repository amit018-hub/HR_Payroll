using System.ComponentModel.DataAnnotations;

namespace HR_Payroll.Core.Models
{
    public class EmployeeBankViewModel
    {
        public int? EmployeeId { get; set; }

        [MaxLength(200)]
        public string BeneficiaryName { get; set; }

        [MaxLength(200)]
        public string BankName { get; set; }

        [MaxLength(50)]
        public string AccountNumber { get; set; }

        [MaxLength(20)]
        public string IFSCCode { get; set; }
    }
}