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
        public string PostalCode { get; set; }
        public bool IsDefault { get; set; }
        public AppUser User { get; set; }
    }
}
