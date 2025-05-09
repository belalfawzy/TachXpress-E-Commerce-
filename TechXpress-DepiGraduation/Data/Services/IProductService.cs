using TechXpress_DepiGraduation.Data.Base;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Services
{
    public interface IProductService : IBaseRepositoryEntity<Product>
    {
        Task CreateAsync(Product product, IFormFile[] imageFiles);
        List<Product> getSimilar(int count,int categoryID);
    }
}
