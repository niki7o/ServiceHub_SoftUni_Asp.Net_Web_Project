using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace ServiceHub.Tests
{
    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryProvider, IAsyncQueryProvider
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            var result = this.AsEnumerable().AsQueryable().Provider.Execute<TResult>(expression);
            return result;
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var syncResult = ((IQueryProvider)this).Execute(expression);

            var taskFromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult))
                                                   .MakeGenericMethod(expectedResultType);

            return (TResult)taskFromResultMethod.Invoke(null, new[] { syncResult });
        }
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }

    public static class TestQueryableExtensions
    {
        public static IQueryable<TEntity> AsQueryable<TEntity>(this IEnumerable<TEntity> source)
        {
            return new TestAsyncEnumerable<TEntity>(source);
        }

        public static IQueryable<TEntity> AsTracking<TEntity>(this IQueryable<TEntity> source)
        {
            return new TestAsyncEnumerable<TEntity>(source.AsEnumerable());
        }
    }
}
