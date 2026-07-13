using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class Seat : BaseEntity
    {
        public string SeatNumber { get; set; }   // A-12 veya B-6 gibi
        public int Floor { get; set; }           //kat bilgisi
        public int PosX { get; set; }            // krokideki konum
        public int PosY { get; set; }

        public ICollection<Reservation> Reservations { get; set; }
    }
}
