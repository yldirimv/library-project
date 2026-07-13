using System.ComponentModel.DataAnnotations;

namespace LibraryProject.ViewModels
{
    public class EmployeeCreateViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
