using System.ComponentModel.DataAnnotations;

namespace LibraryProject.ViewModels
{
    public class EmployeeEditViewModel
    {
        public string Id { get; set; }   

        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin")]
        public string Email { get; set; }
    }
}
