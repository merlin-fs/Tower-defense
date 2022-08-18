using System;
using System.Linq;
using System.Collections.Generic;
using Common.Core;
using Common.Repositories;
using Common.Defs;

namespace Game.Core.Repositories
{
    public class DefsRepository<TDef> : IReadonlyRepository<ObjectTypeID, TDef>
        where TDef : IDef, IIdentifiable<ObjectTypeID>
    {
        public DictionaryRepository<ObjectTypeID, TDef> Repo { get; } = new DictionaryRepository<ObjectTypeID, TDef>();

        #region IReadonlyRepository
        IEnumerable<TDef> IReadonlyRepository<ObjectTypeID, TDef>.Find(
            Func<TDef, bool> filter,
            Func<IQueryable<TDef>, IOrderedQueryable<TDef>> orderBy)
        {
            return Repo.Find(filter, orderBy);
        }

        TDef IReadonlyRepository<ObjectTypeID, TDef>.FindByID(ObjectTypeID id)
        {
            return Repo.FindByID(id);
        }
        #endregion
    }
}

