using LibraryProject.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class NoiseReport : BaseEntity
    {
        public string VisitorId { get; set; }
        public AppUser Visitor { get; set; }
        public int SeatId { get; set; }//ihbarı yapanın koltugu
        public Seat Seat { get; set; }
        public NoiseReportStatus Status { get; set; } = NoiseReportStatus.New;
    }
}
