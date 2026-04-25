using Frock_backend.shared.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories
{
    public class BaseStringRepository<TEntity> : IBaseStringRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext Context;

        protected BaseStringRepository(AppDbContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public async Task AddAsync(TEntity entity)
        {
            await Context.Set<TEntity>().AddAsync(entity);
        }

        /// <inheritdoc />
        public async Task<TEntity?> FindByIdAsync(string id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
        }
        public async Task<TEntity?> FindByIdIntAsync(int id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
        }

        /// <inheritdoc />
        public void Update(TEntity entity)
        {
            Context.Set<TEntity>().Update(entity);
        }

        /// <inheritdoc />
        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> ListAsync()
        {
            return await Context.Set<TEntity>().ToListAsync();
        }
    }
}
