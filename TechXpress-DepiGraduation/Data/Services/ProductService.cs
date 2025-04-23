using TechXpress_DepiGraduation.Data.Base;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Services
{
    public class ProductService : BaseRepositoryEntity<Product>, IProductService
    {
        public ProductService(AppDbContext context) : base(context)
        {
        }
    }
}
