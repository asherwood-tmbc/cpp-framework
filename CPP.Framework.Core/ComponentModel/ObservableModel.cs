using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace CPP.Framework.ComponentModel
{
    /// <summary>
    /// Abstract base class for all data models exposed by the application.
    /// </summary>
    public abstract class ObservableModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The map of property names to values.
        /// </summary>
        private readonly Dictionary<string, object> _propertyValueMap = new Dictionary<string, object>();

        /// <summary>
        /// Occurs when the value of a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the default value for a given type.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <returns>The default value.</returns>
        private TValue GetDefaultValue<TValue>() where TValue : new()
        {
            return (typeof(TValue).IsValueType ? default(TValue) : new TValue());
        }

        /// <summary>
        /// Gets the value for a given property.
        /// </summary>
        /// <param name="expression">An expression that references the property for which to get the value.</param>
        /// <returns>The property value.</returns>
        protected string GetPropertyValue(Expression<Func<string>> expression)
        {
            return this.GetPropertyValue(expression, () => string.Empty);
        }

        /// <summary>
        /// Gets the value for a given property.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">An expression that references the property for which to get the value.</param>
        /// <returns>The property value.</returns>
        protected TValue GetPropertyValue<TValue>(Expression<Func<TValue>> expression) where TValue : new()
        {
            return this.GetPropertyValue(expression, this.GetDefaultValue<TValue>);
        }

        /// <summary>
        /// Gets the value for a given property.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">An expression that references the property for which to get the value.</param>
        /// <param name="factory">A delegate that returns a value for the property if it has not been initialized.</param>
        /// <returns>The property value.</returns>
        protected TValue GetPropertyValue<TValue>(Expression<Func<TValue>> expression, Func<TValue> factory)
        {
            ArgumentValidator.ValidateNotNull(() => expression);
            ArgumentValidator.ValidateNotNull(() => factory);

            var propertyName = expression.GetMemberName();
            if (!_propertyValueMap.TryGetValue(propertyName, out var value))
            {
                _propertyValueMap[propertyName] = value = factory();
                this.OnPropertyChanged(propertyName);
            }
            return (TValue)value;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the value for a given property.
        /// </summary>
        /// <param name="expression">An expression that references the property for which to set the value.</param>
        /// <param name="value">The new value for the property.</param>
        /// <returns>The current property value.</returns>
        protected string SetPropertyValue(Expression<Func<string>> expression, string value)
        {
            return this.SetPropertyValue(expression, value, () => string.Empty);
        }

        /// <summary>
        /// Sets the value for a given property.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">An expression that references the property for which to set the value.</param>
        /// <param name="value">The new value for the property.</param>
        /// <returns>The current property value.</returns>
        protected TValue SetPropertyValue<TValue>(Expression<Func<TValue>> expression, TValue value) where TValue : new()
        {
            return this.SetPropertyValue(expression, value, this.GetDefaultValue<TValue>);
        }

        /// <summary>
        /// Sets the value for a given property.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">An expression that references the property for which to set the value.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="factory">A delegate that returns a value for the property if it has not been initialized.</param>
        /// <returns>The current property value.</returns>
        protected TValue SetPropertyValue<TValue>(Expression<Func<TValue>> expression, TValue value, Func<TValue> factory)
        {
            ArgumentValidator.ValidateNotNull(() => expression);
            ArgumentValidator.ValidateNotNull(() => factory);

            if (ReferenceEquals(null, value))
            {
                value = factory();
            }
            var propertyName = expression.GetMemberName();

            if (!this._propertyValueMap.TryGetValue(propertyName, out var existing) || (!value.Equals(existing)))
            {
                this._propertyValueMap[propertyName] = existing = value;
                this.OnPropertyChanged(propertyName);
            }
            return ((TValue)existing);
        }
    }
}
