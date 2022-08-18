using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Repositories
{
    public interface IReadonlyRepository<TID, TEntity>
    {
        IEnumerable<TEntity> Find(
            Func<TEntity, bool> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null);

        TEntity FindByID(TID id);
    }

    public interface IRepository<TID, TEntity>: IReadonlyRepository<TID, TEntity>
    {
        void Insert(params TEntity[] entities);
        void Insert(IEnumerable<TEntity> entities);

        void Update(params TEntity[] entities);
        void Update(IEnumerable<TEntity> entities);

        void Remove(params TEntity[] entities);
        void Remove(IEnumerable<TEntity> entities);

        TEntity[] Remove(params TID[] ids);
    }
}

