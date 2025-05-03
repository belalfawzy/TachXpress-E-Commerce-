using System.ComponentModel.DataAnnotations;

namespace TechXpress_DepiGraduation.Data.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "this Field is required")]
        [EmailAddress(ErrorMessage = "this Field is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
