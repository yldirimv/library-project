using LibraryProject.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class ReservationEvent : BaseEntity //qr hareketleri burda
    {
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public ReservationEventType EventType { get; set; } // Giriş/MolaBaşla/MolaBitir/Çıkış enumlarım
        public DateTime EventTime { get; set; }
    }
}
