using Basic.Generic.Interface.CRUD;
using Basic.Generic.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading;

namespace Basic.Generic.Repositories
{
    public abstract class BaseRepository<TEntity, TModel, TContext> : IBasicCrud<TModel>
                                                                    where TEntity : EntityWithId
                                                                    where TModel : ModelWithId
                                                                    where TContext : DbContext, new()
    {
        protected IPrincipal CurrentUser => Thread.CurrentPrincipal;

        internal protected TContext Context { get; set; } = new TContext();

        internal protected IQueryable<TEntity> All => Context.Set<TEntity>().AsQueryable();

        private DbSet<TEntity> DbSet => Context.Set<TEntity>();

        internal protected TEntity Find(params object[] keyValues)
        {
            return DbSet.Find(keyValues);
        }

        internal protected IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where(predicate);
        }

        public virtual void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public virtual void Edit(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Insert sans SaveChanges
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal protected virtual TEntity Insert(TEntity entity)
        {
            DbSet.Add(entity);
            return entity;
        }

        /// <summary>
        /// Update sans SaveChanges
        /// Retournes l entité originale si besoin de voir les relations parent - children
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>/!\ renvoie l'objet original pour chainage update</returns>
        internal protected TEntity Update(TEntity entity)
        {
            TEntity pristine = DbSet.Find(entity.Id);
            if (pristine != null)
            {
                Context.Entry(pristine).CurrentValues.SetValues(entity);
            }

            return pristine;
        }

        internal protected int Save()
        {
            return Context.SaveChanges();
        }

        public virtual TEntity Map(TModel model)
        {
            return model.Adapt<TEntity>();
        }

        public virtual TModel Map(TEntity entity)
        {
            return entity.Adapt<TModel>();
        }

        public virtual IEnumerable<TEntity> Map(IEnumerable<TModel> model)
        {
            foreach (TModel m in model)
                yield return Map(m);
        }

        public virtual IEnumerable<TModel> Map(IEnumerable<TEntity> entity)
        {
            foreach (TEntity e in entity)
                yield return Map(e);
        }

        /// <summary>
        /// Insert depuis TModel avec Save
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual Guid Insert(TModel data)
        {
            TEntity e = Insert(Map(data));
            Save();
            return e.Id;
        }

        /// <summary>
        /// Update depuis TModel avec Save
        /// </summary>
        /// <param name="data"></param>
        public virtual void Update(TModel data)
        {
            Update(Map(data));
            Save();
        }

        public void Put(TModel data)
        {
            Put(Map(data));
            Save();
        }

        public virtual void Delete(TModel data, bool realDelete = false)
        {
            if (realDelete)
            {
                Delete(Map(data));
            }
            else
            {
                TEntity s = Find(data.Id);
                s.Deleted = true;
                Update(s);
            }
            Save();
        }

        public void DeleteById(Guid id, bool realDelete = false)
        {
            TEntity s = Find(id);
            if (realDelete)
            {
                Delete(s);
            }
            else
            {
                s.Deleted = true;
                Update(s);
            }
            Save();
        }
        
        public int Count()
        {
            return All.Count();
        }

        public List<TModel> Get(bool includeDelete = false)
        {
            return Map(All.Where(x => x.Deleted == includeDelete)).ToList();
        }

        public TModel Get(Guid id)
        {
            try
            {
                return Map(Find(id));
            }
            catch 
            {
                return null;
            }
        }

        public IEnumerable<TModel> Get(IEnumerable<Guid> ids)
        {
            try
            {
                return DbSet.Where(x => ids.Contains(x.Id)).ToList().Select(x => Map(x));
            }
            catch
            {
                return null;
            }
        }
    }
}
