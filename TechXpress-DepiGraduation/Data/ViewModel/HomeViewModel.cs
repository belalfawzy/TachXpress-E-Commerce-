using System.Collections;
using System.Linq;
using TechXpress_DepiGraduation.Models;


namespace TechXpress_DepiGraduation.Data.ViewModel
{
    public class HomeViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
