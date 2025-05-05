using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Services;

public class PaymentService :IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public  List<AddressView> getAddressesbyUserID(String UserId)
    {
        var addresses = _context.Addresses
            .Where(a => a.UserId == UserId)
           .Select(o=>new AddressView
            {
                Id = o.Id,
                UserId = o.UserId,
                Street = o.Street,
                Country = o.Country,
                City = o.City
            }).ToList() as List<AddressView>;
        return addresses;
    }

}