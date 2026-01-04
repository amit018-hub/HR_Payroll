using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Entity
{
    public class Users
    {
        [Key]
        public int UserID { get; set; }

        public string UserName { get; set; } = null!;

        public string? Email { get; set; }

        public string? MobileNumber { get; set; }

        public string PasswordHash { get; set; } = null!;

        public bool IsEmailVerified { get; set; }

        public bool IsMobileVerified { get; set; }

        public bool IsTwoFactorEnabled { get; set; }

        public string? Status { get; set; }

        public string Del_Flg { get; set; } = "N";

        public DateTime CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool LoggedIn { get; set; }

        public bool AccountLocked { get; set; }

        public int? LoginFailureAttempt { get; set; }

        public int? AccountStatusID { get; set; }

        public DateTime? AccountLockedDate { get; set; }

        public int UserTypeId { get; set; }

        public int? EmailOtpRequiredForLogin { get; set; }

        public int? MobileOtpRequiredForLogin { get; set; }
    }

}
