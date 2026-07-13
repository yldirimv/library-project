using System.ComponentModel.DataAnnotations;

namespace LibraryProject.ViewModels
{
    public class VisitorEditViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır")]
        public string IdentityNumber { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin")]
        public string Email { get; set; }
    }
}
