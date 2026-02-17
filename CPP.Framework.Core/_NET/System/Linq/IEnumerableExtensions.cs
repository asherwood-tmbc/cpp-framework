using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using CPP.Framework;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    /// <summary>
    /// Extension methods for the <see cref="IEnumerable"/> interface.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Performs an action before every element during the enumeration of a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="action">
        /// A delegate that performs an action for each element. Please note that the action will 
        /// not be executed if the sequence contains no elements.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> AfterEvery<TObject>(this IEnumerable<TObject> sequence, Action<TObject> action)
        {
            foreach (var element in Every(sequence, 1, null, action)) yield return element;
        }

        /// <summary>
        /// Performs an action before every element during the enumeration of a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TContext">The type of the user-defined context object.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="action">
        /// A delegate that performs an action for each element. Please note that the action will 
        /// not be executed if the sequence contains no elements.
        /// </param>
        /// <param name="context">
        /// A user-defined context object that passed into the delegate for each invocation.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> AfterEvery<TObject, TContext>(this IEnumerable<TObject> sequence, Action<TObject, TContext> action, TContext context)
        {
            foreach (var element in Every(sequence, 1, null, action, context)) yield return element;
        }

        /// <summary>
        /// Performs an action before every <i>x</i> number of iterations during the enumeration of 
        /// a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="iterations">
        /// The number of iterations between each execution of <paramref name="action"/>. If this 
        /// value is less than or equal to zero, then the action is called for each element in the
        /// sequence.
        /// </param>
        /// <param name="action">
        /// A delegate that performs an action for each cycle. This action is guaranteed to be 
        /// called at least once at the end of the sequence, provided the sequence contains at 
        /// least one element.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> AfterEvery<TObject>(this IEnumerable<TObject> sequence, int iterations, Action<TObject> action)
        {
            foreach (var element in Every(sequence, iterations, null, action)) yield return element;
        }

        /// <summary>
        /// Performs an action before every <i>x</i> number of iterations during the enumeration of 
        /// a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TContext">The type of the user-defined context object.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="iterations">
        /// The number of iterations between each execution of <paramref name="action"/>. If this 
        /// value is less than or equal to zero, then the action is called for each element in the
        /// sequence.
        /// </param>
        /// <param name="action">
        /// A delegate that performs an action for each cycle. This action is guaranteed to be 
        /// called at least once at the end of the sequence, provided the sequence contains at 
        /// least one element.
        /// </param>
        /// <param name="context">
        /// A user-defined context object that passed into the delegate for each invocation.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> AfterEvery<TObject, TContext>(this IEnumerable<TObject> sequence, int iterations, Action<TObject, TContext> action, TContext context)
        {
            foreach (var element in Every(sequence, iterations, null, action, context)) yield return element;
        }

        /// <summary>
        /// Performs an action before every element during the enumeration of a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="action">
        /// A delegate that performs an action for each element, provided sequence 
        /// contains at least one element.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> BeforeEvery<TObject>(this IEnumerable<TObject> sequence, Action<TObject> action)
        {
            foreach (var element in Every(sequence, 1, action, null)) yield return element;
        }

        /// <summary>
        /// Performs an action before every element during the enumeration of a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TContext">The type of the user-defined context object.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="action">
        /// A delegate that performs an action for each element, provided sequence 
        /// contains at least one element.
        /// </param>
        /// <param name="context">
        /// A user-defined context object that passed into the delegate for each invocation.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> BeforeEvery<TObject, TContext>(this IEnumerable<TObject> sequence, Action<TObject, TContext> action, TContext context)
        {
            foreach (var element in Every(sequence, 1, action, null, context)) yield return element;
        }

        /// <summary>
        /// Performs an action before every <i>x</i> number of iterations during the enumeration of 
        /// a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="iterations">
        /// The number of iterations between each execution of <paramref name="action"/>. If this 
        /// value is less than or equal to zero, then the action is called for each element in the
        /// sequence.
        /// </param>
        /// <param name="action">
        /// A delegate that performs an action for each cycle, provided sequence 
        /// contains at least one element.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> BeforeEvery<TObject>(this IEnumerable<TObject> sequence, int iterations, Action<TObject> action)
        {
            foreach (var element in Every(sequence, iterations, action, null)) yield return element;
        }

        /// <summary>
        /// Performs an action before every <i>x</i> number of iterations during the enumeration of 
        /// a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TContext">The type of the user-defined context object.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="iterations">
        /// The number of iterations between each execution of <paramref name="action"/>. If this 
        /// value is less than or equal to zero, then the action is called for each element in the
        /// sequence.
        /// </param>
        /// <param name="action">
        /// A delegate that performs an action for each cycle, provided sequence 
        /// contains at least one element.
        /// </param>
        /// <param name="context">
        /// A user-defined context object that passed into the delegate for each invocation.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> BeforeEvery<TObject, TContext>(this IEnumerable<TObject> sequence, int iterations, Action<TObject, TContext> action, TContext context)
        {
            foreach (var element in Every(sequence, iterations, action, null, context)) yield return element;
        }

        /// <summary>
        /// Returns distinct elements of a sequence using the default equality comparer to compare
        /// values associated with each element.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TValue">The type of the value to compare against.</typeparam>
        /// <param name="sequence">The sequence to remove duplicate elements from.</param>
        /// <param name="selector">A selector function that returns the value to use for filtering duplicate elements.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the filtered sequence.</returns>
        public static IEnumerable<TObject> Distinct<TObject, TValue>(this IEnumerable<TObject> sequence, Func<TObject, TValue> selector)
        {
            //// ReSharper disable PossibleMultipleEnumeration
            ArgumentValidator.ValidateThisObj(() => sequence);
            //// ReSharper restore PossibleMultipleEnumeration

            var hashset = new HashSet<TValue>();
            foreach (var item in sequence)
            {
                if (!ReferenceEquals(null, item))
                {
                    var key = selector(item);
                    if (!hashset.Add(key)) continue;
                }
                yield return item;
            }
        }

        /// <summary>
        /// Enumerates through all the elements in a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        public static void Enumerate<TObject>(this IEnumerable<TObject> sequence)
        {
            foreach (var element in sequence)
            {
                /* ignored */
            }
        }

        /// <summary>
        /// Performs an action before every element during the enumeration of a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="beforeAction">
        /// An optional delegate that is called to perform an action before an element has been
        /// enumerated, provided the sequence contains at least one element.
        /// </param>
        /// <param name="afterAction">
        /// A optional delegate that is called to perform an action after an element has been 
        /// enumerated. This action is guaranteed to be called at least once at the end of the 
        /// sequence, provided the sequence contains at least one element.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> Every<TObject>(this IEnumerable<TObject> sequence, Action<TObject> beforeAction, Action<TObject> afterAction)
        {
            foreach (var element in Every(sequence, 1, beforeAction, afterAction)) yield return element;
        }

        /// <summary>
        /// Performs an action before every element during the enumeration of a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TContext">The type of the user-defined context object.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="beforeAction">
        /// An optional delegate that is called to perform an action before an element has been
        /// enumerated, provided the sequence contains at least one element.
        /// </param>
        /// <param name="afterAction">
        /// A optional delegate that is called to perform an action after an element has been 
        /// enumerated. This action is guaranteed to be called at least once at the end of the 
        /// sequence, provided the sequence contains at least one element.
        /// </param>
        /// <param name="context">
        /// A user-defined context object that passed into the delegate for each invocation.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> Every<TObject, TContext>(
            this IEnumerable<TObject> sequence,
            Action<TObject, TContext> beforeAction,
            Action<TObject, TContext> afterAction,
            TContext context)
        {
            foreach (var element in Every(sequence, 1, beforeAction, afterAction, context)) yield return element;
        }

        /// <summary>
        /// Performs an action before every <i>x</i> number of iterations during the enumeration of 
        /// a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="iterations">
        /// The number of iterations to wait for between calls to <paramref name="beforeAction"/> 
        /// or <paramref name="afterAction"/> delegates. If this value is less than or equal to 
        /// zero, then the action is called for each element in the sequence.
        /// </param>
        /// <param name="beforeAction">
        /// An optional delegate that is called to perform an action before an element has been
        /// enumerated, provided the sequence contains at least one element.
        /// </param>
        /// <param name="afterAction">
        /// A optional delegate that is called to perform an action after an element has been 
        /// enumerated. This action is guaranteed to be called at least once at the end of the 
        /// sequence, provided the sequence contains at least one element.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> Every<TObject>(this IEnumerable<TObject> sequence, int iterations, Action<TObject> beforeAction, Action<TObject> afterAction)
        {
            void BeforeWithContext(TObject obj, object _)
            {
                beforeAction?.Invoke(obj);
            }
            void AfterWithContext(TObject obj, object _)
            {
                afterAction?.Invoke(obj);
            }
            foreach (var element in Every(sequence, iterations, BeforeWithContext, AfterWithContext, (object)null)) yield return element;
        }

        /// <summary>
        /// Performs an action before every <i>x</i> number of iterations during the enumeration of 
        /// a sequence.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <typeparam name="TContext">The type of the user-defined context object.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <param name="iterations">
        /// The number of iterations to wait for between calls to <paramref name="beforeAction"/> 
        /// or <paramref name="afterAction"/> delegates. If this value is less than or equal to 
        /// zero, then the action is called for each element in the sequence.
        /// </param>
        /// <param name="beforeAction">
        /// An optional delegate that is called to perform an action before an element has been
        /// enumerated, provided the sequence contains at least one element.
        /// </param>
        /// <param name="afterAction">
        /// A optional delegate that is called to perform an action after an element has been 
        /// enumerated. This action is guaranteed to be called at least once at the end of the 
        /// sequence, provided the sequence contains at least one element.
        /// </param>
        /// <param name="context">
        /// A user-defined context object that passed into the delegate for each invocation.
        /// </param>
        /// <returns>A copy of the input sequence.</returns>
        public static IEnumerable<TObject> Every<TObject, TContext>(this IEnumerable<TObject> sequence, int iterations, Action<TObject, TContext> beforeAction, Action<TObject, TContext> afterAction, TContext context)
        {
            ArgumentValidator.ValidateThisObj(() => sequence);

            if (iterations <= 0) iterations = 1;    // ensure that the iteration size is always valid
            var count = 0;
            var last = default(TObject);
            foreach (var item in sequence)
            {
                var invoke = ((++count % iterations) == 0);

                if ((beforeAction != null) && invoke) beforeAction(item, context);
                yield return item;
                last = item;
                if ((afterAction != null) && invoke) afterAction(item, context);
            }
            if ((afterAction != null) && (count >= 1) && ((count & iterations) != 0)) afterAction(last, context);
        }

        /// <summary>
        /// Skips over elements in a sequence that are null while it being enumerated.
        /// </summary>
        /// <typeparam name="TObject">The type of the elements in the sequence.</typeparam>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <returns>A copy of the input sequence, with the null values skipped.</returns>
        public static IEnumerable<TObject> SkipNull<TObject>(this IEnumerable<TObject> sequence)
        {
            foreach (var element in sequence.Where(obj => (!object.ReferenceEquals(null, obj))))
            {
                yield return element;
            }
        }

        /// <summary>
        /// Skips over the strings in a sequence that are null or empty while it being enumerated.
        /// </summary>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <returns>A copy of the input sequence, with the null values skipped.</returns>
        public static IEnumerable<string> SkipNullOrEmpty(this IEnumerable<string> sequence)
        {
            foreach (var element in sequence.Where(str => (!string.IsNullOrEmpty(str))))
            {
                yield return element;
            }
        }

        /// <summary>
        /// Skips over the strings in a sequence that are null, empty, or all whitespace characters
        /// while it being enumerated.
        /// </summary>
        /// <param name="sequence">The input sequence being enumerated.</param>
        /// <returns>A copy of the input sequence, with the null values skipped.</returns>
        public static IEnumerable<string> SkipNullOrWhiteSpace(this IEnumerable<string> sequence)
        {
            foreach (var element in sequence.Where(str => (!string.IsNullOrWhiteSpace(str))))
            {
                yield return element;
            }
        }

        /// <summary>
        /// Converts the a sequence of models into a <see cref="DataTable"/> instance.
        /// </summary>
        /// <typeparam name="TObject">The type of the model.</typeparam>
        /// <param name="sequence">The sequence of values to insert into the table.</param>
        /// <returns>A <see cref="DataTable"/> object.</returns>
        public static DataTable ToDataTable<TObject>(this IEnumerable<TObject> sequence)
        {
            const BindingFlags SearchFlags = (BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

            // grab all the public non-indexer and non-collection property definitions for the type.
            var properties = typeof(TObject).GetProperties(SearchFlags)
                .Where(pi => (pi.CanRead))
                .Where(pi => (!typeof(IEnumerable).IsAssignableFrom(pi.PropertyType)))
                .Where(pi => (!pi.GetIndexParameters().Any())).ToList();
            var table = new DataTable();

            // add a column to the data table for each property, and then populate the table.
            var values = new object[properties.Count];
            foreach (var pi in properties)
            {
                table.Columns.Add(pi.Name, pi.PropertyType);
            }
            foreach (var item in sequence)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null);
                }
                table.Rows.Add(values);
            }
            return table;   // return the result
        }

        /// <summary>
        /// Generates a filter against a property whose value is only allowed to be within a given
        /// list of predefined values.
        /// </summary>
        /// <typeparam name="TSource">The type of the object in the sequence.</typeparam>
        /// <typeparam name="TValue">The type of the property to filter.</typeparam>
        /// <param name="source">The source <see cref="IEnumerable{T}"/> sequence to filter.</param>
        /// <param name="expression">
        /// An <see cref="Expression"/> that returns the property to filter.
        /// </param>
        /// <param name="filterValues">
        /// An <see cref="IEnumerable{T}"/> that contains the allowed list of values for the target
        /// property.
        /// </param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that has the filter applied.</returns>
        public static IEnumerable<TSource> WhereAnyOf<TSource, TValue>(this IEnumerable<TSource> source, Expression<Func<TSource, TValue>> expression, IEnumerable<TValue> filterValues)
        {
            if (source is IQueryable<TSource> query)
            {
                return IQueryableExtensions.WhereAnyOf(query, expression, filterValues);
            }
            return WhereAnyOf(source, expression.Compile(), filterValues);
        }

        /// <summary>
        /// Generates a filter against a property whose value is only allowed to be within a given
        /// list of predefined values.
        /// </summary>
        /// <typeparam name="TSource">The type of the object in the sequence.</typeparam>
        /// <typeparam name="TValue">The type of the property to filter.</typeparam>
        /// <param name="source">The source <see cref="IEnumerable{T}"/> sequence to filter.</param>
        /// <param name="expression">
        /// An <see cref="Expression"/> that returns the property to filter.
        /// </param>
        /// <param name="filterValues">
        /// An <see cref="IEnumerable{T}"/> that contains the allowed list of values for the target
        /// property.
        /// </param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that has the filter applied.</returns>
        private static IEnumerable<TSource> WhereAnyOf<TSource, TValue>(IEnumerable<TSource> source, Func<TSource, TValue> expression, IEnumerable<TValue> filterValues)
        {
            ArgumentValidator.ValidateNotNull(() => expression);
            ArgumentValidator.ValidateNotNull(() => filterValues);
            
            if (ReferenceEquals(source, null))
            {
                yield break;
            }
            var filter = new HashSet<TValue>(filterValues);

            foreach (var element in source)
            {
                var candidate = expression(element);
                if (!filter.Contains(candidate))
                {
                    continue;
                }
                yield return element;
            }
        }
    }
}
