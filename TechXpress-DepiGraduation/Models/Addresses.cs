using TechXpress_DepiGraduation.Data.Base;

namespace TechXpress_DepiGraduation.Models
{
    public class Addresses : IBaseEntity
    {
        public int Id { get; set; } 
        public string UserId { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public AppUser User { get; set; }
        public string AddressLine { get; internal set; }

    }
}
