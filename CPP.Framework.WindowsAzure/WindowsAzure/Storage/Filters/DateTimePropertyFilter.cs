using System;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage.Filters
{
    /// <summary>
    /// Represents a filter against a table entity <see cref="DateTime"/> property.
    /// </summary>
    public class DateTimePropertyFilter : TablePropertyFilter<DateTimeOffset>
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the table property.</param>
        /// <param name="operator">A <see cref="ComparisonOperator"/> value that specifies how the property is filtered against the value.</param>
        /// <param name="filterValue">The value to filter against the table property.</param>
        public DateTimePropertyFilter(Expression expression, ComparisonOperator @operator, DateTimeOffset filterValue)
            : base(expression, @operator, filterValue) { }

        /// <summary>
        /// Generates a filter string for the condition.
        /// </summary>
        /// <returns></returns>
        public override string GenerateFilterString()
        {
            var queryOperator = this.Operator.AsQueryOperator();
            return TableQuery.GenerateFilterConditionForDate(this.Property.Name, queryOperator, this.FilterValue);
        }
    }
}
