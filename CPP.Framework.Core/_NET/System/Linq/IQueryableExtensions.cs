using System.Collections.Generic;
using System.Linq.Expressions;

using CPP.Framework;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    /// <summary>
    /// Extension methods for the <see cref="IQueryable"/> and <see cref="IQueryable{T}"/>
    /// interfaces.
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Generates a filter against a property whose value is only allowed to be within a given
        /// list of predefined values.
        /// </summary>
        /// <typeparam name="TSource">The type of the object in the query.</typeparam>
        /// <typeparam name="TValue">The type of the property to filter.</typeparam>
        /// <param name="source">The source <see cref="IQueryable{T}"/> object to filter.</param>
        /// <param name="expression">
        /// An <see cref="Expression"/> that returns the property to filter.
        /// </param>
        /// <param name="filterValues">
        /// An <see cref="IEnumerable{T}"/> that contains the allowed list of values for the target
        /// property.
        /// </param>
        /// <returns>An <see cref="IQueryable{T}"/> object that has the filter applied.</returns>
        public static IQueryable<TSource> WhereAnyOf<TSource, TValue>(this IQueryable<TSource> source, Expression<Func<TSource, TValue>> expression, IEnumerable<TValue> filterValues)
        {
            ArgumentValidator.ValidateNotNull(() => expression);
            ArgumentValidator.ValidateNotNull(() => filterValues);

            var parameter = expression.Parameters.Single();
            var member = ((MemberExpression)expression.Body);
            var generated = default(Expression);

            foreach (var value in filterValues)
            {
                var comparison = Expression.Equal(member, Expression.Constant(value, typeof(TValue)));
                generated = (generated != null) ? Expression.Or(generated, comparison) : comparison;
            }

            var filtered = source;
            if (generated != null)
            {
                var predicate = Expression.Lambda<Func<TSource, bool>>(generated, parameter);
                filtered = filtered.Where(predicate);
            }

            return filtered;
        }
    }
}
