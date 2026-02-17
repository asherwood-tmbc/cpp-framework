using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace CPP.Framework.WindowsAzure.Storage.Filters
{
    /// <summary>
    /// Abstract base class for Windows Azure Table filters that filter against an entity property.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class TablePropertyFilter : AzureTableFilter
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the table property.</param>
        /// <param name="operator">A <see cref="Operator"/> value that specifies how the property is filtered against the value.</param>
        protected TablePropertyFilter(Expression expression, ComparisonOperator @operator)
        {
            ArgumentValidator.ValidateNotNull(() => expression);
            this.Operator = @operator;
            this.Property = ((PropertyInfo)expression.GetMemberInfo());
        }

        /// <summary>
        /// Gets the <see cref="Operator"/> value that specifies how the value is filtered against 
        /// the table property.
        /// </summary>
        public ComparisonOperator Operator { get; private set; }

        /// <summary>
        /// Gets the name of the table property to compare.
        /// </summary>
        public PropertyInfo Property { get; private set; }
    }

    /// <summary>
    /// Abstract base class for Windows Azure Table filters that filter against an entity property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    public abstract class TablePropertyFilter<TValue> : TablePropertyFilter
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the table property.</param>
        /// <param name="operator">A <see cref="ComparisonOperator"/> value that specifies how the property is filtered against the value.</param>
        /// <param name="filterValue">The value to filter against the table property.</param>
        protected TablePropertyFilter(Expression expression, ComparisonOperator @operator, TValue filterValue)
            : base(expression, @operator)
        {
            ArgumentValidator.ValidateNotNull(() => expression);
            this.FilterValue = filterValue;
        }

        /// <summary>
        /// Gets the value to filter against the table property.
        /// </summary>
        public TValue FilterValue { get; private set; }
    }
}
