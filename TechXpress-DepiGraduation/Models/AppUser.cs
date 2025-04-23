using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace TechXpress_DepiGraduation.Models
{
    public class AppUser : IdentityUser
    {
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        public List<Order> Orders { get; set; }
        public List<Addresses> Addresses { get; set; }
    }
}
