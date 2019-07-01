using Basic.Generic.Enum.Pager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Basic.Generic.Repositories.Helpers
{
    public static class OrderByExtensions
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source,
                    Expression<Func<TSource, TKey>> keySelector,
                    SortDirection sortOrder)
        {
            if (sortOrder == SortDirection.Ascending)
                return source.OrderBy(keySelector);
            else
                return source.OrderByDescending(keySelector);
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            SortDirection sortOrder)
        {
            if (sortOrder == SortDirection.Ascending)
                return source.ThenBy(keySelector);
            else
                return source.ThenByDescending(keySelector);
        }

        /// <summary>Orders the sequence by specific column and direction.</summary>
        /// <param name="query">The query.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="ascending">if set to true [ascending].</param>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string sortColumn, SortDirection direction)
        {
            string methodName = string.Format("OrderBy{0}",
                direction == SortDirection.Ascending ? "" : "descending");

            ParameterExpression parameter = Expression.Parameter(query.ElementType, "p");

            MemberExpression memberAccess = null;
            foreach (var property in sortColumn.Split('.'))
                memberAccess = MemberExpression.Property
                   (memberAccess ?? (parameter as Expression), property);

            LambdaExpression orderByLambda = Expression.Lambda(memberAccess, parameter);

            MethodCallExpression result = Expression.Call(
                      typeof(Queryable),
                      methodName,
                      new[] { query.ElementType, memberAccess.Type },
                      query.Expression,
                      Expression.Quote(orderByLambda));

            return query.Provider.CreateQuery<T>(result).OrderBy(x => 0);
        }

        public static IOrderedQueryable<T> OrderBy<T>(
            this IQueryable<T> source,
            IEnumerable<SortDescription> properties)
        {
            // if (properties == null || properties.Any()) return source;

            var typeOfT = typeof(T);

            Type t = typeOfT;

            IOrderedQueryable<T> result = null;
            var thenBy = false;

            foreach (var item in properties
                .Select(prop => new { PropertyInfo = t.GetProperty(prop.PropertyName), prop.Direction }))
            {
                var oExpr = Expression.Parameter(typeOfT, "o");
                var propertyInfo = item.PropertyInfo;
                var propertyType = propertyInfo.PropertyType;
                var isAscending = item.Direction == SortDirection.Ascending;

                if (thenBy)
                {
                    var prevExpr = Expression.Parameter(typeof(IOrderedQueryable<T>), "prevExpr");
                    var expr1 = Expression.Lambda<Func<IOrderedQueryable<T>, IOrderedQueryable<T>>>(
                        Expression.Call(
                            (isAscending ? thenByMethod : thenByDescendingMethod).MakeGenericMethod(typeOfT, propertyType),
                            prevExpr,
                            Expression.Lambda(
                                typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(oExpr, propertyInfo),
                                oExpr)
                            ),
                        prevExpr)
                        .Compile();

                    result = expr1(result);
                }
                else
                {
                    var prevExpr = Expression.Parameter(typeof(IEnumerable<T>), "prevExpr");
                    var expr1 = Expression.Lambda<Func<IEnumerable<T>, IOrderedQueryable<T>>>(
                        Expression.Call(
                            (isAscending ? orderByMethod : orderByDescendingMethod).MakeGenericMethod(typeOfT, propertyType),
                            prevExpr,
                            Expression.Lambda(
                                typeof(Func<,>).MakeGenericType(typeOfT, propertyType),
                                Expression.MakeMemberAccess(oExpr, propertyInfo),
                                oExpr)
                            ),
                        prevExpr)
                        .Compile();

                    result = expr1(source);
                    thenBy = true;
                }
            }
            return result;
        }

        private static readonly MethodInfo orderByMethod =
            MethodOf(() => Enumerable.OrderBy(default, default(Func<object, object>)))
                .GetGenericMethodDefinition();

        private static readonly MethodInfo orderByDescendingMethod =
            MethodOf(() => Enumerable.OrderByDescending(default, default(Func<object, object>)))
                .GetGenericMethodDefinition();

        private static readonly MethodInfo thenByMethod =
            MethodOf(() => Enumerable.ThenBy(default, default(Func<object, object>)))
                .GetGenericMethodDefinition();

        private static readonly MethodInfo thenByDescendingMethod =
            MethodOf(() => Enumerable.ThenByDescending(default, default(Func<object, object>)))
                .GetGenericMethodDefinition();

        public static MethodInfo MethodOf<T>(Expression<Func<T>> method)
        {
            MethodCallExpression mce = (MethodCallExpression)method.Body;
            MethodInfo mi = mce.Method;
            return mi;
        }
    }
}
