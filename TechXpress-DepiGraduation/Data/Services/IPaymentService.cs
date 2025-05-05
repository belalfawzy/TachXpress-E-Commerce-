using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Services;

public interface IPaymentService
{
    List<AddressView> getAddressesbyUserID(String UserId);
    
}