using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Default policy used to select the constructor when building up instances of an object
    /// through the <see cref="ServiceLocator"/>.
    /// </summary>
    internal sealed class ConstructorSelectorPolicy : IConstructorSelectorPolicy
    {
        private static readonly ConstructorLengthComparer ConstructorComparer = new ConstructorLengthComparer();

        /// <summary>
        /// Creates a new <see cref="IDependencyResolverPolicy"/> for a given constructor parameter.
        /// </summary>
        /// <param name="parameter">The parameter to create the resolver for.</param>
        /// <returns>An <see cref="IDependencyResolverPolicy"/> object.</returns>
        private IDependencyResolverPolicy CreateResolver(ParameterInfo parameter)
        {
            ArgumentValidator.ValidateNotNull(() => parameter);
            var attribute = parameter.GetCustomAttributes(false)
                .OfType<DependencyResolutionAttribute>()
                .FirstOrDefault();
            if (attribute != null)
            {
                return attribute.CreateResolver(parameter.ParameterType);
            }
            return new NamedTypeDependencyResolverPolicy(parameter.ParameterType, null);
        }

        /// <summary>
        /// Searches for set of constructors that match a given filter criteria.
        /// </summary>
        /// <param name="typeToConstruct">The target type to search.</param>
        /// <param name="selector">A delegate that is used to filter the results.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object.</returns>
        private static IEnumerable<ConstructorInfo> FindConstructor(Type typeToConstruct, Func<ConstructorInfo, bool> selector = null)
        {
            selector = (selector ?? ((ci) => true));
            if (typeToConstruct != null)
            {
                var info = typeToConstruct.GetTypeInfo();
                return info.DeclaredConstructors.Where(ctor => (!ctor.IsStatic)).Where(selector);
            }
            return Enumerable.Empty<ConstructorInfo>();
        }

        /// <summary>
        /// Searches for a constructor with the <see cref="InjectionConstructorAttribute"/> applied.
        /// </summary>
        /// <param name="typeToConstruct">The target type to search.</param>
        /// <returns>
        /// A <see cref="ConstructorInfo"/> object, or null if no constructors were found.
        /// </returns>
        private static ConstructorInfo FindInjectionConstructor(Type typeToConstruct)
        {
            var array = FindConstructor(
                typeToConstruct, 
                (ctor) =>
                    {
                        if (!ctor.IsDefined(typeof(InjectionConstructorAttribute)))
                        {
                            return ctor.IsDefined(typeof(ServiceLocatorConstructorAttribute), true);
                        }
                        return true;
                    })
                .ToArray();
            switch (array.Length)
            {
                case 0: return null;
                case 1: return array[0];
                default:
                    {
                        var message = string.Format(
                            CultureInfo.CurrentCulture,
                            ErrorStrings.MultipleInjectionConstructors,
                            typeToConstruct.GetTypeInfo().Name);
                        throw new InvalidOperationException(message);
                    }
            }
        }

        /// <summary>
        /// Searches for the constructor with the highest number of parameters.
        /// </summary>
        /// <param name="typeToConstruct">The target type to search.</param>
        /// <returns>
        /// A <see cref="ConstructorInfo"/> object, or null if no constructors were found.
        /// </returns>
        private static ConstructorInfo FindLongestConstructor(Type typeToConstruct)
        {
            var array = FindConstructor(typeToConstruct, (ctor) => ctor.IsPublic).ToArray();
            Array.Sort(array, ConstructorComparer);
            switch (array.Length)
            {
                case 0: return null;
                case 1: return array[0];
                default:
                    {
                        var length = array[0].GetParameters().Length;
                        if (array[1].GetParameters().Length == length)
                        {
                            var message = string.Format(
                                CultureInfo.CurrentCulture,
                                ErrorStrings.AmbiguousInjectionConstructor,
                                typeToConstruct.GetTypeInfo().Name,
                                length);
                            throw new InvalidOperationException(message);
                        }
                        return array[0];
                    }
            }
        }

        /// <summary>
        /// Choose the constructor to call for the given type.
        /// </summary>
        /// <param name="context">The current build context</param>
        /// <param name="policies">
        /// The <see cref="IPolicyList" /> to add any generated resolver objects into.
        /// </param>
        /// <returns>The chosen constructor.</returns>
        public SelectedConstructor SelectConstructor(IBuilderContext context, IPolicyList policies)
        {
            ArgumentValidator.ValidateNotNull(() => context);
            
            var typeToConstruct = context.BuildKey.Type;
            var ctor = (FindInjectionConstructor(typeToConstruct) ?? FindLongestConstructor(typeToConstruct));

            if (ctor != null)
            {
                var selected = new SelectedConstructor(ctor);
                foreach (var parameter in ctor.GetParameters())
                {
                    var resolver = this.CreateResolver(parameter);
                    selected.AddParameterResolver(resolver);
                }
                return selected;
            }
            return default(SelectedConstructor);
        }

        #region ConstructorLengthComparer Class Declaration

        /// <summary>
        /// <see cref="IComparer{T}"/> implementation used to sort <see cref="ConstructorInfo"/>
        /// objects by their parameter count in descending order.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private class ConstructorLengthComparer : IComparer<ConstructorInfo>
        {
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal
            /// to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            ///     <list type="table">
            ///         <listheader>
            ///             <term>Value</term>
            ///             <description>Condition</description>
            ///         </listheader>
            ///         <item>
            ///             <term>Less than zero</term>
            ///             <description><paramref name="x"/> is less than <paramref name="y"/>.</description>
            ///         </item>
            ///         <item>
            ///             <term>Zero</term>
            ///             <description><paramref name="x"/> equals <paramref name="y"/>.</description>
            ///         </item>
            ///         <item>
            ///             <term>Greater than zero</term>
            ///             <description><paramref name="x"/> is greater than <paramref name="y"/>.</description>
            ///         </item>
            ///     </list>
            /// </returns>
            [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Reviewed. Suppression is OK here.")]
            public int Compare(ConstructorInfo x, ConstructorInfo y)
            {
                ArgumentValidator.ValidateNotNull(() => x);
                ArgumentValidator.ValidateNotNull(() => y);
                return (y.GetParameters().Length - x.GetParameters().Length);
            }
        }

        #endregion // ConstructorLengthComparer Class Declaration
    }
}
