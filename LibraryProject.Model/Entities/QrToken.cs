using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class QrToken : BaseEntity
    {
        public string Token { get; set; } // rastgele üretilen değer
        public DateTime ExpiresAt { get; set; } //refresh zamanı 
        public bool IsUsed { get; set; } //token tek seferlik olsun dıye
    }
}
