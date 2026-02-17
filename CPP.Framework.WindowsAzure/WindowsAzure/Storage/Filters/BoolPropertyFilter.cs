using System;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage.Filters
{
    /// <summary>
    /// Represents a filter against a table entity <see cref="string"/> property.
    /// </summary>
    public class BoolPropertyFilter : TablePropertyFilter<Boolean>
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the table property.</param>
        /// <param name="operator">A <see cref="ComparisonOperator"/> value that specifies how the property is filtered against the value.</param>
        /// <param name="filterValue">The value to filter against the table property.</param>
        public BoolPropertyFilter(Expression expression, ComparisonOperator @operator, Boolean filterValue)
            : base(expression, @operator, filterValue) { }

        /// <summary>
        /// Generates a filter string for the condition.
        /// </summary>
        /// <returns></returns>
        public override string GenerateFilterString()
        {
            var queryOperator = this.Operator.AsQueryOperator();
            return TableQuery.GenerateFilterConditionForBool(this.Property.Name, queryOperator, this.FilterValue);
        }
    }
}
