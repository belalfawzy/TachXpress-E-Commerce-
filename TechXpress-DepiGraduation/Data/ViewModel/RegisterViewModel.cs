using System.ComponentModel.DataAnnotations;

namespace TechXpress_DepiGraduation.Data.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "this Field is required")]
        [EmailAddress(ErrorMessage = "this Field is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least 8 characters, including one letter, one number, and one special character.")]
        public string Password { get; set; }


        [Required(ErrorMessage = "this Field is required")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "this Field is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full Name must contain only letters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "this Field is required")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(11, ErrorMessage = "Phone Number must be 11 digits")]
        [RegularExpression(@"^01\d{9}$", ErrorMessage = "Phone number must be 11 digits and start with 01")]
        public string PhoneNumber { get; set; }
    }
}
