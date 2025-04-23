namespace TechXpress_DepiGraduation.Data.Base
{
    public interface IBaseRepositoryEntity<T> where T : class , IBaseEntity
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetItemByIdAsync(int id);
        Task CreateAsync(T entity);
        Task EditAsync(T Enitity);
        Task DeleteAsync(int id);

    }
}
