using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage.Entities
{
    /// <summary>
    /// Represents an entity in table storage.
    /// </summary>
    /// <typeparam name="TModel">The type of the entity model.</typeparam>
    public abstract class AzureTableEntity<TModel> : AzureTableEntity where TModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableEntity{TModel}"/> class.
        /// </summary>
        /// <param name="model">The object to assign to the entity model.</param>
        /// <param name="automatic">True to automatically serialize model properties using reflection; otherwise, false.</param>
        protected AzureTableEntity(TModel model, bool automatic)
        {
            ArgumentValidator.ValidateNotNull(() => model);
            this.Automatic = automatic;
            this.Model = model;
        }

        /// <summary>
        /// Gets a value indicating whether or not the model's properties are automatically
        /// serialized and deserialized using reflection when the entity is read or written to 
        /// Windows Azure storage.
        /// </summary>
        protected bool Automatic { get; }

        /// <summary>
        /// Gets the model associated with the entity.
        /// </summary>
        [IgnoreProperty]
        public TModel Model { get; }

        /// <summary>
        /// Gets the <see cref="MemberInfo"/> for a given property or field on the model.
        /// </summary>
        /// <typeparam name="TValue">The return type of the field or property.</typeparam>
        /// <param name="expression">An expression that evaluates to a field or property defined by <typeparamref name="TModel"/>.</param>
        /// <returns>A <see cref="PropertyInfo"/> instance.</returns>
        protected static PropertyInfo GetModelPropertyInfo<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var propertyInfo = (expression.GetMemberInfo() as PropertyInfo);
            if (propertyInfo != null)
            {
                return propertyInfo;
            }
            throw ArgumentValidator.CreateArgumentExceptionFor(() => expression, ErrorStrings.InvalidPropertyAccessExpression);
        }

        /// <summary>
        /// Called by the base class to determine whether or not a property can be automatically 
        /// serialized or deserialized.
        /// </summary>
        /// <param name="propertyInfo">
        /// The <see cref="PropertyInfo"/> for the property to serialize or deserialize.
        /// </param>
        /// <returns>
        /// True if <paramref name="propertyInfo"/> can be automatically serialized and 
        /// deserialized; otherwise, false.
        /// </returns>
        protected virtual bool CanSerializeProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetIndexParameters().Any()) return false;
            if (propertyInfo.GetCustomAttributes<IgnorePropertyAttribute>().Any()) return false;
            if ((!propertyInfo.CanRead) || (!propertyInfo.CanWrite)) return false;
            return true;
        }

        /// <summary>
        /// Called by the base class to load the properties for the model from the values in table 
        /// storage.
        /// </summary>
        /// <param name="serializer">The <see cref="AzureTableEntity{TModel}.PropertySerializer"/> used to store the property values.</param>
        protected virtual void LoadModelProperties(PropertySerializer serializer) { }

        /// <summary>
        /// Populates the entity's properties from the <see cref="EntityProperty"/> data values in 
        /// the <paramref name="properties"/> dictionary. 
        /// </summary>
        /// <param name="properties">The dictionary of string property names to <see cref="EntityProperty"/> data values to deserialize and store in this table entity instance.</param>
        /// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
        protected override sealed void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            var serializer = new PropertySerializer(this.Model, properties);
            if (this.Automatic)
            {
                foreach (var propertyInfo in this.Model.GetType().GetProperties())
                {
                    if (!this.CanSerializeProperty(propertyInfo))
                    {
                        continue;
                    }
                    serializer.Load(this.Model, propertyInfo);
                }
            }
            this.LoadModelProperties(serializer);
        }

        /// <summary>
        /// Called by the base class to save the properties for the model before persisting it to 
        /// table storage.
        /// </summary>
        /// <param name="serializer">The <see cref="AzureTableEntity{TModel}.PropertySerializer"/> used to store the property values.</param>
        protected virtual void SaveModelProperties(PropertySerializer serializer) { }

        /// <summary>
        /// Serializes the <see cref="IDictionary{TKey,TValue}"/> of property names mapped to 
        /// <see cref="EntityProperty"/> data values from the entity instance.
        /// </summary>
        /// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> object of property names to <see cref="EntityProperty"/> data typed values created by serializing this table entity instance.</returns>
        protected override sealed IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var serializer = new PropertySerializer(this.Model);

            if (this.Automatic)
            {
                foreach (var propertyInfo in this.Model.GetType().GetProperties())
                {
                    if (!this.CanSerializeProperty(propertyInfo))
                    {
                        continue;
                    }
                    serializer.Save(this.Model, propertyInfo);
                }
            }
            this.SaveModelProperties(serializer);

            return serializer.ToDictionary();
        }

        #region PropertySerializer Class Declaration

        /// <summary>
        /// Represents a property serializer for the current table.
        /// </summary>
        protected sealed class PropertySerializer : AzureTableEntitySerializer<TModel>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PropertySerializer"/> class.
            /// </summary>
            /// <param name="model">The model being serialized or deserialized.</param>
            internal PropertySerializer(TModel model) : base(model) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertySerializer"/> class.
            /// </summary>
            /// <param name="model">The entity associated with the model whose properties are being serialized or deserialized.</param>
            /// <param name="properties">An optional dictionary that contains the existing property values.</param>
            internal PropertySerializer(TModel model, IDictionary<string, EntityProperty> properties) : base(model, properties) { }
        }

        #endregion // PropertySerializer Class Declaration
    }
}
