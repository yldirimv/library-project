using LibraryProject.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryProject.Model.Entities
{
    public class BookLoan : BaseEntity
    {
        public int BookId { get; set; }
        public Book Book { get; set; }
        public string VisitorId { get; set; }
        public AppUser Visitor { get; set; } //appuser ilişkisi 2 tane. dbcontext'te ef icin ayrıca tanımlanacak 
        public string EmployeeId { get; set; }    //ödüncü kaydeden personel
        public AppUser Employee { get; set; }

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }     //loanDate + 20 gün
        public DateTime? ReturnDate { get; set; } 
        public LoanStatus Status { get; set; } = LoanStatus.Active;
    }
}
