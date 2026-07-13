using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Dtos
{
    public class SeatStateDto
    {
        public string State { get; set; } = "free";//reserved/occupied/break
        public string? VisitorName { get; set; }
        public string? TimeRange { get; set; }
    }
}
