using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LibraryProject.Model.Entities
{
    public class Book : BaseEntity
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string? ISBN { get; set; }
        public int TotalStock { get; set; }   //musait adet hesabı totalStock-aktif ödünc ile yapılcak extra gerek yok

        public ICollection<BookLoan> Loans { get; set; }
    }
}
