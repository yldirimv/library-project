using System.ComponentModel.DataAnnotations;

namespace LibraryProject.ViewModels
{
    public class LoginViewModel
    //Login formunun veritabanında karşılığı yok (şifre + beni hatırla diye tablo tutmuyoruz);
    //register'da ise AppUser'ı doğrudan forma bağlamak güvenlik açığıdır
    //kötü niyetli biri form POST'una NoShowCount=0 gibi alanlar enjekte edebilir (overposting saldırısı).
    //Form için ayrı, sadece formun alanlarını taşıyan sınıflar en temiz yol
    {
        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
