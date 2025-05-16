using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.ViewModel
{
    public class OrderCheckoutViewModel
    {
        public List<Addresses> UserAddresses { get; set; }
        public List<ShoppingCartItem> CartItems { get; set; }
        public decimal Total { get; set; }
        public Addresses NewAddress { get; set; } = new Addresses();
    }
}
