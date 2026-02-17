using System.Diagnostics.CodeAnalysis;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage.Filters
{
    /// <summary>
    /// Defines the possible operators for joining the results of two table filters.
    /// </summary>
    public enum CombineOperator
    {
        /// <summary>
        /// Represents an AND join condition.
        /// </summary>
        And,
        /// <summary>
        /// Represents an OR join condition.
        /// </summary>
        Or,
        /// <summary>
        /// Represents the NOT join condition.
        /// </summary>
        Not,
    }
    
    /// <summary>
    /// Extensions methods for the <see cref="ComparisonOperator"/> type.
    /// </summary>
    public static class CombineOperatorExtensions
    {
        /// <summary>
        /// Translate a <see cref="ComparisonOperator"/> value to its string representation.
        /// </summary>
        /// <param name="operator">The operator to translate.</param>
        /// <returns>The string representation of <paramref name="operator"/>.</returns>
        [ExcludeFromCodeCoverage]
        public static string AsQueryOperator(this CombineOperator @operator)
        {
            switch (@operator)
            {
                case CombineOperator.And: return TableOperators.And;
                case CombineOperator.Not: return TableOperators.Not;
                case CombineOperator.Or: return TableOperators.Or;
                default: throw ArgumentValidator.CreateArgumentExceptionFor(() => @operator, "Invalid join operator \"{0}\".", @operator);
            }
        }
    }
}