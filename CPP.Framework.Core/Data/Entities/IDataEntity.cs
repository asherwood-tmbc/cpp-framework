using System;
using System.ComponentModel;
using CPP.Framework.Collections;

namespace CPP.Framework.Data.Entities
{
    /***
     * Note: In order to work properly with the Microsoft OData client, the key property has to be
     * named "ID," otherwise the client will throw exceptions when materializing responses, even 
     * when the entity is being redirected to an actual concrete client entity type. The only other
     * alternative is to decorate the interface with the DataServiceKey attribute, but that would 
     * require a reference to the OData library, which we can't do in an interfaces assembly.
     */

    /// <summary>
    /// Abstract interface for all entities stored in a data source.
    /// </summary>
    public interface IDataEntity : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the globally unique identifier of the entity.
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// Gets the property map for the entity collections defined by the current entity.
        /// </summary>
        ICollectionPropertyMap CollectionPropertyMap { get; }
    }
}
