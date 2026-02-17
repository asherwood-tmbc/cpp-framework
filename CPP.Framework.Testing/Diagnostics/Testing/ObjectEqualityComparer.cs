using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// Compares the property and field for a object of a given type to determine if their values 
    /// are equal.
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    [ExcludeFromCodeCoverage]
    public sealed class ObjectEqualityComparer<TObject> : IEqualityComparer<TObject> where TObject : class
    {
        private static readonly ObjectEqualityComparer<TObject> _Default = new ObjectEqualityComparer<TObject>();

        private readonly List<PropertyInfo> _PropInfoList = new List<PropertyInfo>();
        private readonly List<FieldInfo> _VarsInfoList = new List<FieldInfo>();
 
        /// <summary>
        /// Initializes the static members of the class.
        /// </summary>
        private ObjectEqualityComparer()
        {
            _VarsInfoList.AddRange(typeof(TObject)
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .OfType<FieldInfo>()
                .Where(vars => (!vars.IsLiteral)));
            _PropInfoList.AddRange(typeof(TObject)
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .OfType<PropertyInfo>()
                .Where(prop => (prop.CanRead)));
        }

        /// <summary>
        /// Gets the default comparer for the given type.
        /// </summary>
        public static ObjectEqualityComparer<TObject> Default { get { return _Default; } }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <typeparamref name="TObject"/> to compare.</param>
        /// <param name="y">The second object of type <typeparamref name="TObject"/> to compare.</param>
        /// <returns>True if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(TObject x, TObject y)
        {
            if ((x == null) && (y == null)) return true;
            if ((x != null) && (y == null)) return false;
            if ((x == null) && (y != null)) return false;
            var count = 0L;

            count = _PropInfoList
                .Count(info => (Object.Equals(info.GetValue(x), info.GetValue(y))));
            if (count != _PropInfoList.Count) return false;

            count = _VarsInfoList
                .Count(info => (Object.Equals(info.GetValue(x), info.GetValue(y))));
            if (count != _VarsInfoList.Count) return false;

            return true;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="ArgumentNullException">The type of <paramref name="obj"/> is null.</exception>
        public int GetHashCode(TObject obj)
        {
            ArgumentValidator.ValidateNotNull(() => obj);
            return obj.GetHashCode();
        }
    }
}
