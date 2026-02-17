using System;

namespace CPP.Framework.WindowsAzure
{
    /// <summary>
    /// Abstract interface for message classes that support delivery through the
    /// <see cref="AzureTableQueue{TModel,TEntity}"/> derived classes.
    /// </summary>
    public interface IAzureTableQueueModel
    {
        /// <summary>
        /// Gets the date and time when the message should be delivered by the queue.
        /// </summary>
        DateTime? ScheduledDeliveryDate { get; }
    }
}
