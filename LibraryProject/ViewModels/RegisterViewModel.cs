using System.ComponentModel.DataAnnotations;

namespace LibraryProject.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        public string FullName { get; set; }

        //koseli parantezler DATA ANNOTATION yani dogrulama kuralları
        //controllerdaki ModelState.IsValid bu kuralları isletir. viewdaki asp-validation-for da hatayı kullanıcıya basar

        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır")]
        public string IdentityNumber { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; }
    }
}
