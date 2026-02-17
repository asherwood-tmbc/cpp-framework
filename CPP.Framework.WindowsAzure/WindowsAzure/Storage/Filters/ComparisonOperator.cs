using System.Diagnostics.CodeAnalysis;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage.Filters
{
    /// <summary>
    /// Defines the available operators for a filter.
    /// </summary>
    public enum ComparisonOperator
    {
        /// <summary>
        /// Represents the Equal operator.
        /// </summary>
        Equal,
        /// <summary>
        /// Represents the Not Equal operator.
        /// </summary>
        NotEqual,
        /// <summary>
        /// Represents the Greater Than operator.
        /// </summary>
        GreaterThan,
        /// <summary>
        /// Represents the Greater Than or Equal operator.
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// Represents the Less Than operator.
        /// </summary>
        LessThan,
        /// <summary>
        /// Represents the Less Than or Equal operator.
        /// </summary>
        LessThanOrEqual,
    }

    /// <summary>
    /// Extension methods for the <see cref="ComparisonOperator"/> type.
    /// </summary>
    public static class ComparisonOperatorExtensions
    {
        /// <summary>
        /// Translate a <see cref="ComparisonOperator"/> value to its string representation.
        /// </summary>
        /// <param name="operator">The operator to translate.</param>
        /// <returns>The string representation of <paramref name="operator"/>.</returns>
        [ExcludeFromCodeCoverage]
        public static string AsQueryOperator(this ComparisonOperator @operator)
        {
            switch (@operator)
            {
                case ComparisonOperator.Equal: return QueryComparisons.Equal;
                case ComparisonOperator.GreaterThan: return QueryComparisons.GreaterThan;
                case ComparisonOperator.GreaterThanOrEqual: return QueryComparisons.GreaterThanOrEqual;
                case ComparisonOperator.LessThan: return QueryComparisons.LessThan;
                case ComparisonOperator.LessThanOrEqual: return QueryComparisons.LessThanOrEqual;
                case ComparisonOperator.NotEqual: return QueryComparisons.NotEqual;
                default: throw ArgumentValidator.CreateArgumentExceptionFor(() => @operator, "Invalid query operator \"{0}\".", @operator);
            }
        }
    }
}
