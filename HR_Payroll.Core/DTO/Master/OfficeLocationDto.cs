using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.Core.DTO.Master
{
    public class OfficeLocationDto
    {
        public int OfficeID { get; set; }
        public string? OfficeName { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? GeoFenceRadius { get; set; }
        public bool IsActive { get; set; }
    }
}
