using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace HR_Payroll.Core.Models
{
    public class EmployeeBasicInfoViewModel
    {
        public int? EmployeeId { get; set; }

        [MaxLength(50)]
        public string? EmployeeCode { get; set; }

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public int? DepartmentId { get; set; }
        public int? SubDepartmentId { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        public DateTime? JoiningDate { get; set; }

        [MaxLength(200)]
        public string? ReportingTo { get; set; }

        [MaxLength(50)]
        public string? SourceOfHire { get; set; }

        [MaxLength(200)]
        public string? Interviewer { get; set; }

        [MaxLength(200)]
        public string? AttendanceRules { get; set; }

        public int? EmploymentStatus { get; set; }

        [MaxLength(20)]
        public string? MaritalStatus { get; set; }

        [MaxLength(20)]
        public string? AadharNo { get; set; }

        [MaxLength(20)]
        public string? PANNo { get; set; }

        [MaxLength(50)]
        public string? PFNo { get; set; }

        [MaxLength(50)]
        public string? UANNo { get; set; }

        [MaxLength(50)]
        public string?   ESINo { get; set; }

        public int? NoticePeriod { get; set; }

        // File
        public IFormFile? ProfilePicture { get; set; }
    }
}