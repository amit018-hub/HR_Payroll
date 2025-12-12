using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Entity
{
    public class Employees
    {
        [Key]
        public int EmployeeID { get; set; }
        public int UserID { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? DepartmentId { get; set; }
        public string? Designation { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? OrganisationName { get; set; }
        public int CountryID { get; set; }
        public int StateID { get; set; }
        public int DistrictID { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? PinCode { get; set; }
        public string? ProfilePic { get; set; }
        public int? SubDepartmentId { get; set; }
        public int? SupervisorID { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsActive { get; set; }
        public string? Del_Flg { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
    }
}
