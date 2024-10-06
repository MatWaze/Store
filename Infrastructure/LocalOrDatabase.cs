using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq;
using System.Linq.Expressions;

namespace Store.Infrastructure
{
    public static class LocalOrDatabase
    {
        public static IEnumerable<EntityEntry<T>> Local<T>(this DbContext context)
            where T : class
        {
            return context.ChangeTracker.Entries<T>();
        }

        public static IEnumerable<EntityEntry<T>> Local<T>(this DbSet<T> set)
            where T : class
        {
            if (set is InternalDbSet<T>)
            {
                Console.WriteLine("Local");
                var svcs = (set as InternalDbSet<T>)
                    .GetInfrastructure()
                    .GetService<IDbContextServices>();
                var ctx = svcs.CurrentContext.Context;
                return Local<T>(ctx);
            }
            throw new ArgumentException("Invalid set", "set");
        }

        public static IQueryable<T> LocalOrDb<T>(this DbContext context) where T : class
        {
            var localResults = context.Set<T>().Local.Where(c => true);

            if (localResults.Any() == true)
            {
                Console.WriteLine(localResults.Count());
                Console.WriteLine("trying to get products from cache");
                return localResults.AsQueryable();
            }
           
            IQueryable<T> databaseResults = context.Set<T>().Where(c => true);
            return databaseResults;
        }

    }
}
