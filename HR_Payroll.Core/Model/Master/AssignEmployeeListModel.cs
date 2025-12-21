using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model.Master
{
    public class AssignEmployeeListModel
    {
        public int TotalRecords { get; set; }
        public int SlNo { get; set; }
        public int EmployeeID { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Department { get; set; }
        public string? SubDepartment { get; set; }
        public string? EmployeeName { get; set; }
        public string? TeamLead { get; set; }
        public string? Manager { get; set; }
        public bool IsActive { get; set; }
        public DateTime? JoiningDate { get; set; }
    }
}
