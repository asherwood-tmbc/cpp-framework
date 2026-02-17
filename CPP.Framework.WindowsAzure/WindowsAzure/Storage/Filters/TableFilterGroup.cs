using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage.Filters
{
    /// <summary>
    /// Abstract base class for filters that combine the results of two other filters.
    /// </summary>
    public class TableFilterGroup : AzureTableFilter
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="filterA">The first <see cref="AzureTableFilter"/> in the group.</param>
        /// <param name="operator">A <see cref="CombineOperator"/> value that indicates how to combine <paramref name="filterA"/> and <paramref name="filterB"/>.</param>
        /// <param name="filterB">The second <see cref="AzureTableFilter"/> in the group.</param>
        public TableFilterGroup(AzureTableFilter filterA, CombineOperator @operator, AzureTableFilter filterB)
        {
            ArgumentValidator.ValidateNotNull(() => filterA);
            ArgumentValidator.ValidateNotNull(() => filterB);
            this.FilterA = filterA;
            this.FilterB = filterB;
            this.Operator = @operator;
        }

        /// <summary>
        /// Gets the first <see cref="AzureTableFilter"/> in the group.
        /// </summary>
        public AzureTableFilter FilterA { get; private set; }

        /// <summary>
        /// Gets the second <see cref="AzureTableFilter"/> in the group.
        /// </summary>
        public AzureTableFilter FilterB { get; private set; }

        /// <summary>
        /// Gets the operator used to join the <see cref="FilterA"/> and <see cref="FilterB"/> filters.
        /// </summary>
        public CombineOperator Operator { get; private set; }

        /// <summary>
        /// Generates a filter string for the condition.
        /// </summary>
        /// <returns></returns>
        public override string GenerateFilterString()
        {
            var filters = new[] { this.FilterA.GenerateFilterString(), this.FilterB.GenerateFilterString(), };
            var @operator = this.Operator.AsQueryOperator();
            return TableQuery.CombineFilters(filters[0], @operator, filters[1]);
        }
    }
}
