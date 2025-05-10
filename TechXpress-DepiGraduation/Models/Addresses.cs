using System.ComponentModel.DataAnnotations;
using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    public class Addresses : IBaseEntity
    {
        public int Id { get; set; } 
        public string UserId { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Country must contain only letters")]
        public string Country { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "City Must be have at least 3 chars")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "City must contain only letters")]
        public string City { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Street Must be have at least 3 chars")]
        public string Street { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Postal Code Must be 5 digits")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Postal Code must be 5 digits")]
        [DataType(DataType.PostalCode)]
         public string ZipCode { get; set; }
        public AppUser User { get; set; }
        [Required(ErrorMessage = "this Field is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Please enter detailed information for ex. 16 El Galaa St.")]
        public string AddressLine { get;set; }

    }
}
