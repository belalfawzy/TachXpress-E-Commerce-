using System.ComponentModel.DataAnnotations;

namespace TechXpress_DepiGraduation.Data.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "this Field is required")]
        [EmailAddress(ErrorMessage = "this Field is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(11, ErrorMessage = "Phone Number must be 11 digits")]
        public string PhoneNumber { get; set; }
    }
}
