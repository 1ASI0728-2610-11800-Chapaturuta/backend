namespace Frock_backend.shared.Domain.Repositories
{

    //CREE ESTE REPOSITORY BASE PARA PODER SER UTILIZADO POR LOS REPOSITORIOS QUE TENGAN COMO ID UN STRING
    // Y ASI NO TENER QUE REPETIR EL CODIGO EN CADA UNO DE LOS REPOSITORIOS QUE LO NECESITEN
    // EN ESTE CASO SOLO PARA REGIONS, PROVINCES, DISTRICTS Y LOCALITIES
    //XDD
    public interface IBaseStringRepository<TEntity>
    {
        Task AddAsync(TEntity entity);
        Task<TEntity?> FindByIdAsync(string id);
        Task<TEntity?> FindByIdIntAsync(int id);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        Task<IEnumerable<TEntity>> ListAsync();
    }
}
