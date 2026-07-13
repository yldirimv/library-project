using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LibraryProject.Model.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? IdentityNumber { get; set; }        // TC olucak
        public int NoShowCount { get; set; }               // 3 olunca 1 hfata rez yasak
        public DateTime? ReservationBanUntil { get; set; } // rezervasyon yasağı bitişi
        public DateTime? LoanBanUntil { get; set; }        // kitap yasağı bitiş

        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<BookLoan> Loans { get; set; }
        public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; }
    }
}
