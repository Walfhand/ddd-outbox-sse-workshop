using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Models;

namespace Engine.Web;

public static class PaginationQueryableExtensions
{
    extension<T>(IQueryable<T> query)
    {
        public async Task<PaginatedResult<TResult>> ToPaginatedResultAsync<TResult>(int pageIndex,
            int pageSize,
            Func<T, TResult> map,
            string? sortBy = null,
            bool ascending = true,
            CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(sortBy)) query = query.OrderByDynamic(sortBy, ascending);

            var totalCount = await query.CountAsync(cancellationToken);
            var pageItems = await query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => map(x))
                .ToListAsync(cancellationToken);

            return new PaginatedResult<TResult>(pageItems, totalCount, pageIndex, pageSize);
        }

        private IQueryable<T> OrderByDynamic(string sortBy,
            bool ascending)
        {
            var param = Expression.Parameter(typeof(T), "x");

            var property = sortBy.Split('.').Aggregate<string?, Expression>(param, Expression.Property!);

            var lambda = Expression.Lambda(property, param);

            var methodName = ascending ? "OrderBy" : "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type);

            return (IQueryable<T>)method.Invoke(null, [query, lambda])!;
        }
    }
}