using Basic.Generic.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Basic.Generic.Repositories
{
    public interface IGenericRepository<TEntity, TModel> where TEntity : EntityWithId where TModel : ModelWithId
    {
        Guid Insert(TModel entity);
        void Update(TModel entity);
        void Delete(TModel data);
        void Delete(Guid id);
        IList<TModel> GetAll();
        int Count();
    }

    public abstract class BaseRepository<TEntity, TModel, TContext> : IGenericRepository<TEntity, TModel>, IDisposable
                                                                    where TEntity : EntityWithId
                                                                    where TModel : ModelWithId
                                                                    where TContext : DbContext, new()
    {
        internal protected TypeAdapterConfig ToEntityConfig = new TypeAdapterConfig();
        internal protected TypeAdapterConfig ToModelConfig = new TypeAdapterConfig();
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
            return model.Adapt<TEntity>(ToEntityConfig);
        }

        public virtual TModel Map(TEntity entity)
        {
            return entity.Adapt<TModel>(ToModelConfig);
        }

        public virtual IEnumerable<TEntity> Map(IEnumerable<TModel> model)
        {
            return model.Adapt<IEnumerable<TEntity>>();
        }

        public virtual IEnumerable<TModel> Map(IEnumerable<TEntity> entity)
        {
            return entity.Adapt<IEnumerable<TModel>>();
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

        public virtual void Delete(TModel data)
        {
            Delete(Map(data));
            Save();
        }

        public virtual void Delete(Guid key)
        {
            TEntity entity = Find(key);
            Delete(entity);
            Save();
        }

        public IList<TModel> GetAll()
        {
            return Map(All.ToList()).ToList();
        }

        public int Count()
        {
            return All.Count();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Context.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                Context = null;
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseRepository() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }


        #endregion
    }
}
