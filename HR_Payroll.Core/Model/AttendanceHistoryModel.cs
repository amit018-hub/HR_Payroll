using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.Model
{
    public class AttendanceHistoryModel
    {
        public int HistoryID { get; set; }
        public int AttendanceID { get; set; }
        public string? ActionType { get; set; }
        public DateTime? ActionTime { get; set; }
        public TimeSpan? PunchTime { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Location { get; set; }
        public string? Address { get; set; }
        public int IsWithinGeofence { get; set; }
        public decimal DistanceFromOffice { get; set; }
        public string? Remarks { get; set; }
        public string? IPAddress { get; set; }
        public string? DeviceInfo { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }

}
