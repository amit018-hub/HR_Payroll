using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO
{
    public class EmployeeBasicDto
    {
        public int EmployeeID { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? SubDepartmentId { get; set; }
        public string? SubDepartmentName { get; set; }
        public string? Reporting_To { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? ProfilePic { get; set; }
        public string? AadhaarNo { get; set; }
        public string? PANNo { get; set; }
        public string? PFNo { get; set; }
        public string? UANNo { get; set; }
        public string? ESINo { get; set; }
    }
}
