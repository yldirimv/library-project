using LibraryProject.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class LoyaltyTransaction : BaseEntity
    {
        public string VisitorId { get; set; }
        public AppUser Visitor { get; set; }
        public int Points { get; set; }          
        public LoyaltySource Source { get; set; } // Oturum / HaftalıkBonus / HediyeHarcama enum
        public string? Description { get; set; }  // 5 saat çalışma - 50 puan gibi
    }
}
