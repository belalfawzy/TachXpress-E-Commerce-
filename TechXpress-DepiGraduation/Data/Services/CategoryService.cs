using TechXpress_DepiGraduation.Data.Base;
using TechXpress_DepiGraduation.Models;

namespace TechXpress_DepiGraduation.Data.Services
{
    public class CategoryService : BaseRepositoryEntity<Category>, ICategoryService
    {
        public CategoryService(AppDbContext context) : base(context)
        {
        }
    }
}
