using LibraryProject.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class GiftRequest : BaseEntity
    {
        public int GiftId { get; set; }
        public Gift Gift { get; set; }
        public string VisitorId { get; set; }
        public AppUser Visitor { get; set; }
        public GiftRequestStatus Status { get; set; } = GiftRequestStatus.Pending;
        public DateTime? ProcessedDate { get; set; }  // admin onay zamannı
    }
}
