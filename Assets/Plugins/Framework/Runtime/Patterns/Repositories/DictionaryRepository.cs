using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.Repositories
{
    using Core;

    [Serializable]
    public sealed class DictionaryRepository<TID, TEntity> : IRepository<TID, TEntity>
        where TEntity: IIdentifiable<TID>
    {
        [SerializeField]
        private readonly Dictionary<TID, TEntity> m_Items;

        public DictionaryRepository()
        {
            m_Items = new Dictionary<TID, TEntity>();
        }

        public DictionaryRepository(Dictionary<TID, TEntity> items)
        {
            m_Items = items;
        }

        #region IRepository<TID, TEntity>
        public IEnumerable<TEntity> Find(
            Func<TEntity, bool> filter,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            IEnumerable<TEntity> qry = m_Items.Values;

            if (filter != null)
                qry = qry
                    .Where(filter)
                    .AsEnumerable();

            if (orderBy != null)
                qry = orderBy?.Invoke(qry.AsQueryable());

            return qry;
        }

        public TEntity FindByID(TID id)
        {
            return m_Items.TryGetValue(id, out TEntity value) 
                ? value 
                : default;
        }

        public void Insert(TID id, TEntity entity)
        {
            if (m_Items.ContainsKey(id))
                return;
            m_Items.Add(id, entity);
        }

        public void Insert(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                m_Items.Add(entity.ID, entity);
            }
        }
        public void Insert(params TEntity[] entities)
        {
            Insert(entities as IEnumerable<TEntity>);
        }

        public void Update(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                m_Items[entity.ID] = entity;

        }
        public void Update(params TEntity[] entities)
        {
            Update(entities as IEnumerable<TEntity>);
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                m_Items.Remove(entity.ID);
        }
        public void Remove(params TEntity[] entities)
        {
            Remove(entities as IEnumerable<TEntity>);
        }

        public TEntity[] Remove(params TID[] ids)
        {
            List<TEntity> result = new List<TEntity>();
            foreach (var id in ids)
            {
                if (m_Items.TryGetValue(id, out TEntity found))
                {
                    result.Add(found);
                    m_Items.Remove(id);
                }
            }
            return result.ToArray();
        }
        #endregion

        public void Clear()
        {
            m_Items.Clear();
        }
    }
}

