using LibraryProject.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class Reservation : BaseEntity
    {
        public string VisitorId { get; set; }     //visitorId string a92ub22bgUG84b30h gibi
        public AppUser Visitor { get; set; }
        public int SeatId { get; set; }
        public Seat Seat { get; set; }

        public DateTime StartTime { get; set; }  
        public DateTime EndTime { get; set; }     
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public ICollection<ReservationEvent> Events { get; set; }
    }
}
