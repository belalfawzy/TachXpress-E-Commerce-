using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TechXpress_DepiGraduation.Models;
namespace TechXpress_DepiGraduation.Data.Base
{
    public class BaseRepositoryEntity<T> : IBaseRepositoryEntity<T> where T : class, IBaseEntity
    {
        protected readonly AppDbContext _context;
        public BaseRepositoryEntity(AppDbContext context)
        {
            _context = context;
        }
        public async Task CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Set<T>().FirstOrDefaultAsync(i => i.Id == id);
            if(entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task EditAsync(T Entity)
        {
            EntityEntry entityEntry = _context.Entry<T>(Entity);
            entityEntry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<T>> GetAllAsync()
            => await _context.Set<T>().ToListAsync();
        

        public async Task<T> GetItemByIdAsync(int id)
            => await _context.Set<T>().FirstOrDefaultAsync(i => i.Id == id);
    }
}
