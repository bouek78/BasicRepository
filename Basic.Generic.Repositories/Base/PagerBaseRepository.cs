using Basic.Generic.Common.Pager;
using Basic.Generic.Enum.Pager;
using Basic.Generic.Interface.CRUD;
using Basic.Generic.Interface.Pager;
using Basic.Generic.Models;
using Basic.Generic.Repositories.Helpers;
using Mapster;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Basic.Generic.Repositories.Base
{
    /// <summary>
    /// Classe pour le repository avec CRUD / Pagination / Vue
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TView"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public abstract class PagerBaseRepository<TEntity, TModel, TView, TViewModel, TContext> : PagerBaseRepository<TEntity, TModel, TContext>
        where TEntity : EntityWithId, ISortable, ISelectable, IFilterable, IOrderable
        where TModel : ModelWithId
        where TContext : DbContext, new()
        where TView : EntityWithId, ISortable, ISelectable, IFilterable, IOrderable
        where TViewModel : ModelWithId
    {
        public new virtual PagerList<TViewModel> GetPager(PagerQuery query)
        {
            return GetPagerWithWhere<TView, TViewModel>(query, a => !a.Deleted);
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
        where TEntity : EntityWithId, ISortable, ISelectable, IFilterable, IOrderable
        where TModel : ModelWithId
        where TContext : DbContext, new()
        where TViewModel : ModelWithId
    {
        public new virtual PagerList<TViewModel> GetPager(PagerQuery query)
        {
            return GetPagerWithWhere<TEntity, TViewModel>(query, a => !a.Deleted);
        }
    }

    /// <summary>
    /// Classe pour le repository avec CRUD / Pagination
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public abstract class PagerBaseRepository<TEntity, TModel, TContext> : BaseRepository<TEntity, TModel, TContext>, IBasicCrud<TModel>
        where TEntity : EntityWithId, ISortable, ISelectable, IFilterable, IOrderable
        where TModel : ModelWithId
        where TContext : DbContext, new()
    {
        public virtual PagerList<TModel> GetPager(PagerQuery query)
        {
            return GetPagerWithWhere<TEntity, TModel>(query, a => !a.Deleted);
        }

        public virtual Expression<Func<TSource, bool>> GetSearchFilter<TSource>(string searchKey)
            where TSource : IFilterable
        {
            if (!String.IsNullOrEmpty(searchKey))
                return e => e.Name.Contains(searchKey);
            else
                return e => true;             
        }

        public virtual Expression<Func<TSource, TResult>> GetSelect<TSource, TResult>()
            where TSource : ISelectable
            where TResult : class
        {
            return s => s.Adapt<TResult>();
        }

        public virtual (Expression<Func<TSource, string>> keySelector, SortDirection sort) GetOrderBy<TSource>()
            where TSource : ISortable
        {
            return (e => e.Name, SortDirection.Ascending);
        }

        public virtual IEnumerable<(Expression<Func<TSource, string>> keySelector, SortDirection sort)> GetThenBy<TSource>()
        {
            return null;
        }

        internal protected PagerList<TResult> GetPagerWithWhere<TSource, TResult>(IPagerQuery query, params Expression<Func<TSource, object>>[] includes)
            where TSource : EntityWithId, ISortable, ISelectable, IFilterable, IOrderable
            where TResult : ModelWithId
        {
            Expression<Func<TSource, bool>> filter = GetSearchFilter<TSource>(query.SearchKey);

            /// COUNT
            int resultCount = CountFindBySearchKey(filter);
            query = AdjustPageNumber(query, resultCount);

            /// WHERE
            IQueryable<TSource> q = QueryableFindBySearchKey(filter, includes);

            /// ORDER BY
            IOrderedQueryable<TSource> orderedQuery = QueryableOrderBy(q, query);

            /// TAKE / SKIP
            IQueryable<TSource> orderedFilteredQuery = orderedQuery
                                                        .Skip(query.SkipValue)
                                                        .Take(query.ItemsPerPage);

            /// SELECT
            List<TResult> model = orderedFilteredQuery.Select(GetSelect<TSource, TResult>()).ToList();

            return new PagerList<TResult>
            {
                List = model,
                PagerQuery = query
            };
        }

        internal protected IQueryable<TModel> QueryableSelect(IQueryable<TEntity> q, IPagerQuery query)
        {
            return q.Select(GetSelect<TEntity, TModel>());
        }

        internal protected IOrderedQueryable<TSource> QueryableOrderBy<TSource>(IQueryable<TSource> q, IPagerQuery query)
            where TSource : ISortable
        {
            if (query.HasSortingCondition)
            {
                if (!String.IsNullOrEmpty(query.SortColumnName))
                    return q.OrderBy(query.SortColumnName, query.SortDirection);
                else
                    return q.OrderBy(query.SortDescription);
            }

            (Expression<Func<TSource, string>> keySelector, SortDirection sort) = GetOrderBy<TSource>();
            IOrderedQueryable<TSource> ord = q.OrderBy(keySelector, sort);
            if (GetThenBy<TSource>() != null)
            {
                foreach((Expression<Func<TSource, string>> k, SortDirection s) in GetThenBy<TSource>())
                {
                    ord = ord.ThenBy(k, s);
                }
            }

            return ord;
        }

        internal protected int CountFindBySearchKey<TSource>(Expression<Func<TSource, bool>> where)
            where TSource : EntityWithId, ISelectable
        {
            return QueryableFindBySearchKey(where).Count();
        }

        internal protected virtual IQueryable<TSource> QueryableFindBySearchKey<TSource>(Expression<Func<TSource, bool>> where)
            where TSource : EntityWithId, ISelectable
        {
            return QueryableFindBySearchKey(where, null);
        }

        internal protected virtual IQueryable<TSource> QueryableFindBySearchKey<TSource>(Expression<Func<TSource, bool>> where, params Expression<Func<TSource, object>>[] includes)
            where TSource : EntityWithId, ISelectable
        {
            var dbset = Context.Set<TSource>().AsQueryable();

            if (includes != null && includes.Any())
            {
                foreach (var path in includes)
                {
                    dbset.Include(path).Load();
                }
            }
            return dbset.AsNoTracking().Where(where);
        }

        internal protected IPagerQuery AdjustPageNumber(IPagerQuery query, int resultCount)
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
