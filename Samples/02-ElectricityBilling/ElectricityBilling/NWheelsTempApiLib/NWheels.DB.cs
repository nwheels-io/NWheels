using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels
{
    namespace DB
    {
        public unsafe struct PrimaryKeyType
        {
            public fixed byte Bytes[16];
        }

        public static class TypeContract
        {
            public class ViewAttribute : Attribute
            {
                public ViewAttribute(Type over)
                {
                }
            }
        }

        public static class MemberContract
        {
            public class MapToMemberAttribute : Attribute
            {
                public MapToMemberAttribute(Type type, string memberName)
                {
                }
            }
            public class ManyToOneAttribute : Attribute
            {
            }
        }

        public static class EnumerableExtensions
        {
            public static IAsyncEnumerable<T> AsAsync<T>(this IEnumerable<T> source)
            {
                throw new NotImplementedException();
            }
        }

        public interface IAsyncEnumerator<out T> : IDisposable
        {
            Task ResetAsync();
            Task<bool> MoveNextAsync();
            T Current { get; }
        }

        public interface IAsyncEnumerable<T>
        {
            IAsyncEnumerable<T> Take(long count);
            IAsyncEnumerable<T> While(Func<T, Task<bool>> predicateAsync, Func<T, Task> actionAsync);
            IAsyncEnumerable<T> Skip(long count);
            IAsyncEnumerable<T> SkipWhile(Func<T, Task<bool>> predicateAsync);
            IAsyncEnumerable<T> SkipWhile(Func<T, Task<bool>> predicateAsync, out long skippedCount);
            IAsyncEnumerable<TOther> Cast<TOther>();
            IAsyncEnumerable<TOther> OfType<TOther>();
            Task<IAsyncEnumerator<T>> GetEnumeratorAsync();
            Task ForEachAsync(Func<T, Task> actionAsync);
            Task ForEachAsync(Action<T> action);
            Task<bool> AnyAsync();
            Task<long> CountAsync();
            Task<T> FirstAsync();
            Task<T> FirstOrDefaultAsync();
            Task<T> LastAsync();
            Task<T> LastOrDefaultAsync();
            Task<List<T>> ToListAsync();
            Task<Dictionary<TKey, T>> ToDictionaryAsync<TKey>(Func<T, TKey> keySelector);
            Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector);
            Task<T[]> ToArrayAsync();
        }

        public static class AsyncEnumerableExtensions
        {
            public static Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(
                this IAsyncEnumerable<KeyValuePair<TKey, TValue>> query)
            {
                return query.ToDictionaryAsync<TKey, TValue>(x => x.Key, x => x.Value);
            }
        }

        public interface IAsyncGrouping<TKey, TElement> : IAsyncEnumerable<TElement>
        {
            TKey Key { get; }
        }

        public interface IAsyncQuery<T> : IAsyncEnumerable<T>
        {
            IAsyncQuery<T> Where(Expression<Func<T, bool>> predicate);
            IAsyncQuery<IAsyncGrouping<TKey, T>> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector);
            IOrderedAsyncQuery<T> OrderBy<TField>(Expression<Func<T, TField>> field);
            IOrderedAsyncQuery<T> OrderByDescending<TField>(Expression<Func<T, TField>> field);
        }

        public interface IOrderedAsyncQuery<T> : IAsyncEnumerable<T>
        {
            IOrderedAsyncQuery<T> ThenBy<TField>(Expression<Func<T, TField>> field);
            IOrderedAsyncQuery<T> ThenByDescending<TField>(Expression<Func<T, TField>> field);
        }

        public interface IRepository<T> : IEnumerable<T>
        {
            IAsyncQuery<TResult> Query<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
            T New(Func<T> constructor);
            void Delete(T obj);
            void BulkUpdate(Expression<Func<T, bool>> where, Action<IUpdateBuilder<T>> update);
            void BulkDelete(Expression<Func<T, bool>> where);
        }

        public interface IUpdateBuilder<T>
        {
            IUpdateBuilder<T> Set<TValue>(Expression<Func<T, TValue>> member, Expression<Func<TValue>> value);
        }

        public interface IView<T>
        {
            IAsyncQuery<T> Query(Func<IQueryable<T>, IQueryable<T>> query);
        }
    }
}
