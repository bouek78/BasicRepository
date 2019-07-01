using Basic.Generic.Common.Pager;
using Basic.Generic.Interface.CRUD;
using Basic.Generic.Interface.Pager;
using Basic.Generic.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Repositories.Base
{
        public interface IPagerRepository<TEntity>
            where TEntity : EntityWithId, ISortable
        {
            Expression<Func<TEntity, bool>> GetSearchFilter(string searchKey);
        }

        /// <summary>
        /// Classe pour le repository avec CRUD / Pagination / Vue
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TView"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        public abstract class PagerBaseRepository<TEntity, TModel, TView, TViewModel, TContext> : PagerBaseRepository<TEntity, TModel, TContext>
            where TEntity : EntityWithId, ISortable
            where TModel : ModelWithId
            where TContext : DbContext, new()
            where TView : EntityWithId, ISortable
            where TViewModel : ModelWithId
    {
            public new virtual Expression<Func<TView, bool>> GetSearchFilter(string searchKey)
            {
                return e => e.Name.Contains(searchKey);
            }

            protected virtual PagerList<TViewModel> GetPagerWithWhere(PagerQuery query, Expression<Func<TView, bool>> filter)
            {
                if (!string.IsNullOrEmpty(query.SearchKey))
                    filter = filter.AndAlso(GetSearchFilter(query.SearchKey));

                int resultCount = CountFindBySearchKey(filter);
                query = AdjustPageNumber(query, resultCount);

                IQueryable<TView> queryStart = QueryableFindBySearchKey(filter)
                    .OrderBy(query.SortColumnName, query.SortDirection).AsQueryable();

                IEnumerable<TView> datas = queryStart
                    .Skip(() => query.SkipValue)
                    .Take(() => query.ItemsPerPage);

                List<TViewModel> dataList = datas.Select(data =>
                {
                    data.Adapt<TViewModel>();                    
                }).ToList();

                return new PagerList<TViewModel>
                {
                    TransientsList = dataList,
                    PagerQuery = query
                };
            }
        }

        /// <summary>
        /// Classe pour le repository avec CRUD / Pagination / Vue
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TView"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        public abstract class PagerBaseRepository<TEntity, TModel, TViewModel, TContext> : PagerBaseRepository<TEntity, TModel, TContext>
            where TEntity : EntityWithId, ISortable
            where TModel : ModelWithId
            where TContext : DbContext, new()
            where TViewModel : ModelWithId
    {
            public new virtual Expression<Func<TEntity, bool>> GetSearchFilter(string searchKey)
            {
                return e => e.Name.Contains(searchKey);
            }

            public virtual Func<TEntity, TViewModel> Select()
            {
                return s => s.Adapt<TViewModel>();
            }

            protected new virtual PagerList<TViewModel> GetPagerWithWhere(PagerQuery query, Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includes)
            {
                if (!string.IsNullOrEmpty(query.SearchKey))
                    filter = filter.AndAlso(GetSearchFilter(query.SearchKey));

                int resultCount = CountFindBySearchKey(filter);
                query = AdjustPageNumber(query, resultCount);

                IQueryable<TEntity> queryStart = QueryableFindBySearchKey(filter, includes);

                if (!query.SortColumnName.Contains("&"))
                    queryStart = queryStart.OrderBy(query.SortColumnName, query.SortDirection.ToString()).AsQueryable();
                else
                    queryStart = GetComplexOrder(queryStart, query);

                IEnumerable<TEntity> datas = queryStart
                    .Skip(() => query.SkipValue)
                    .Take(() => query.ItemsPerPage);

                List<TViewModel> dataList = datas.Select(Select()).ToList();

                return new PagerList<TViewModel>
                {
                    TransientsList = dataList,
                    PagerQuery = query
                };
            }
        }

        /// <summary>
        /// Classe pour le repository avec CRUD / Pagination
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TContext"></typeparam>
        public abstract class PagerBaseRepository<TEntity, TModel, TContext> : BaseRepository<TEntity, TModel, TContext>, IPagerRepository<TEntity>, IBasicCrud<TModel>
            where TEntity : EntityWithId, ISortable
            where TModel : ModelWithId
            where TContext : DbContext, new()
        {
            public virtual PagerList<TModel> GetPager(PagerQuery query)
            {
                return GetPagerWithWhere(query, a => !a.Deleted);
            }

            public virtual Expression<Func<TView, bool>> GetSearchFilter<TView>(string searchKey)
                where TView : EntityWithId, ISortable
            {
                // defaut, retourne Name
                return e => e.Name.Contains(searchKey);
            }

            public virtual Expression<Func<TEntity, bool>> GetSearchFilter(string searchKey)
            {
                return GetSearchFilter<TEntity>(searchKey);
            }

            public virtual IQueryable<TEntity> GetComplexOrder(IQueryable<TEntity> source, PagerQuery query)
            {
                List<SortDescription> sort = new List<SortDescription>();
                foreach (string name in query.SortColumnName.Split('&'))
                {
                    if (name.Contains('|'))
                    {
                        Enum.TryParse(name.Split('|')[1], out SortDirection dir);
                        sort.Add(new SortDescription(name.Split('|')[0], dir));
                    }
                    else
                    {
                        sort.Add(new SortDescription(name, query.SortDirection));
                    }
                }

                return source.BuildOrderBys(sort).AsQueryable();
            }

            protected virtual PagerList<TModel> GetPagerWithWhere(PagerQuery query, Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includes)
            {
                if (!string.IsNullOrEmpty(query.SearchKey))
                    filter = filter.AndAlso(GetSearchFilter(query.SearchKey));

                int resultCount = CountFindBySearchKey(filter);
                query = AdjustPageNumber(query, resultCount);

                IQueryable<TEntity> queryStart = QueryableFindBySearchKey(filter, includes);

                if (!query.SortColumnName.Contains("&"))
                    queryStart = queryStart.OrderBy(query.SortColumnName, query.SortDirection.ToString()).AsQueryable();
                else
                    queryStart = GetComplexOrder(queryStart, query);

                //IOrderedEnumerable<TEntity> orderedQuery = queryStart.OrderBy(query.SortColumnName, query.SortDirection);

                IEnumerable<TEntity> datas = queryStart
                    .Skip(query.SkipValue)
                    .Take(query.ItemsPerPage);

                List<TModel> dataList = datas.Select(data =>
                {
                    TModel model = Map(data);
                    model.IsNew = false;
                    return model;
                }).ToList();

                return new PagerListTransient<TModel>
                {
                    TransientsList = dataList,
                    PagerQuery = query
                };
            }

            protected int CountFindBySearchKey<TView>(Expression<Func<TView, bool>> where)
                where TView : EntityWithId, ISortable
            {
                return QueryableFindBySearchKey(where).Count();
            }

            protected int CountFindBySearchKey(Expression<Func<TEntity, bool>> where)
            {
                return CountFindBySearchKey<TEntity>(where);
            }

            protected virtual IQueryable<TView> QueryableFindBySearchKey<TView>(Expression<Func<TView, bool>> where)
                where TView : EntityWithId, ISortable
            {
                return Context.Set<TView>().AsNoTracking().Where(where);
            }

            protected virtual IQueryable<TEntity> QueryableFindBySearchKey(Expression<Func<TEntity, bool>> where)
            {
                return QueryableFindBySearchKey(where, null);
            }

            protected virtual IQueryable<TEntity> QueryableFindBySearchKey(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includes)
            {
                var dbset = Context.Set<TEntity>().AsQueryable();

                if (includes != null && includes.Any())
                {
                    foreach (var path in includes)
                    {
                        dbset.Include(path).Load();
                    }
                }
                return dbset.Where(where);
            }

            protected PagerQuery AdjustPageNumber(PagerQuery query, int resultCount)
            {
                int firstItemIndexInPage = (query.CurrentIndex - 1) * query.ItemsPerPage + 1;
                if (firstItemIndexInPage > resultCount && query.CurrentIndex > 1)
                {
                    query.CurrentIndex = (int)Math.Ceiling((decimal)resultCount / query.ItemsPerPage);
                }
                query.TotalItemsCount = resultCount;

                return query;
            }
        }
   
}
