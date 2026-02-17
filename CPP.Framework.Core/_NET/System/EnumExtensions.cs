using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Extension methods for the system <see cref="Enum"/> class.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description for a given enum value.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="value">The value of the enum.</param>
        /// <returns>A string that contains the description of <paramref name="value"/>.</returns>
        public static string GetDescription<T>(this T value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return value.GetDescription(",");
        }

        /// <summary>
        /// Gets the description for a given enum value.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="value">The value of the enum.</param>
        /// <param name="separator">The string separator to use when joining descriptions if <paramref name="value"/> is a combination of more than one enum value (if the enum has the <see cref="FlagsAttribute"/>)).</param>
        /// <returns>A string that contains the description of <paramref name="value"/>.</returns>
        public static string GetDescription<T>(this T value, string separator)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            if (Enum.IsDefined(value.GetType(), value))
            {
                var description = (GetEnumCustomAttributes<DescriptionAttribute>(value)
                    .Select(attr => attr.Description)
                    .FirstOrDefault() ?? Enum.GetName(value.GetType(), value));
                return description;
            }
            return string.Join(separator, value.Split().Select(x => x.GetDescription()).OrderBy(s => s));
        }

        /// <summary>
        /// Retrieves the <see cref="EditorBrowsableState"/> value for an enum member.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="value">The value of the enum.</param>
        /// <returns>The <see cref="EditorBrowsableState"/> value.</returns>
        public static EditorBrowsableState GetEditorBrowsableState<T>(this T value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            EditorBrowsableState state;
            if (Enum.IsDefined(value.GetType(), value))
            {
                state = (GetEnumCustomAttributes<EditorBrowsableAttribute>(value)
                    .Select(attr => attr.State)
                    .DefaultIfEmpty(EditorBrowsableState.Always)
                    .FirstOrDefault());
            }
            else
            {
                state = value.Split()
                    .Select(x => x.GetEditorBrowsableState())
                    .DefaultIfEmpty(EditorBrowsableState.Always).Aggregate(
                        EditorBrowsableState.Always,
                        (seed, current) =>
                            {
                                switch (seed)
                                {
                                    case EditorBrowsableState.Advanced:
                                        {
                                            switch (current)
                                            {
                                                case EditorBrowsableState.Never: return current;
                                            }
                                        }
                                        break;
                                    case EditorBrowsableState.Always:
                                        {
                                            switch (current)
                                            {
                                                case EditorBrowsableState.Advanced:
                                                case EditorBrowsableState.Never: return current;
                                            }
                                        }
                                        break;
                                    case EditorBrowsableState.Never: break;
                                }
                                return seed;
                            });
            }
            return state;
        }

        /// <summary>
        /// Retrieves a sequence of custom attributes of a given type that have been applied to an
        /// enum value.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="value">The enum value.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterate over the sequence.</returns>
        private static IEnumerable<TAttribute> GetEnumCustomAttributes<TAttribute>(object value)
            where TAttribute : Attribute
        {
            var enumtype = value.GetType();
            if (Enum.IsDefined(enumtype, value))
            {
                var name = Enum.GetName(enumtype, value);
                var attributes = (enumtype.GetField(name)
                    .GetCustomAttributes(typeof(TAttribute), false)
                    .OfType<TAttribute>());
                return attributes;
            }
            return Enumerable.Empty<TAttribute>();
        }

        /// <summary>
        /// Gets the fully qualified name of an enum value, including the namespace and enum
        /// type name.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="value">The enum value.</param>
        /// <returns>The fully qualified name for <paramref name="value"/>.</returns>
        public static string GetFullName<T>(this T value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return $"{typeof(T).FullName}.{Enum.GetName(typeof(T), value)}";
        }

        /// <summary>
        /// Determines whether or not an enum type is marked with the <see cref="FlagsAttribute"/>
        /// attribute.
        /// </summary>
        /// <param name="type">The type of the enum.</param>
        /// <returns>True if <paramref name="type"/> represents a flags enum; otherwise, false.</returns>
        public static bool IsFlagsEnum(Type type)
        {
            return type.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
        }

        /// <summary>
        /// Determines whether or not an enum type is marked with the <see cref="FlagsAttribute"/>
        /// attribute.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <returns>True if <typeparamref name="T"/> represents a flags enum; otherwise, false.</returns>
        public static bool IsFlagsEnum<T>() where T : struct, IConvertible, IFormattable, IComparable
        {
            return IsFlagsEnum(typeof(T));
        }

        /// <summary>
        /// Splits a combined enum value into its individual flag values. If the enum is not marked
        /// with the <see cref="FlagsAttribute"/>, then only the value of the <paramref name="value"/>
        /// parameter is contained in the returned sequence.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="value">The value to split.</param>
        /// <returns>A sequence that contains the individual flags for <paramref name="value"/>.</returns>
        public static IEnumerable<T> Split<T>(this T value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            var combined = Convert.ToInt64(value);
            if ((combined == 0) && Enum.IsDefined(value.GetType(), 0))
            {
                yield return value;
                yield break;
            }
            foreach (var candidate in Enum.GetValues(value.GetType()))
            {
                var mask = Convert.ToInt64(candidate);
                if (mask == 0) continue;
                if ((mask & combined) == mask) yield return ((T)candidate);
                combined &= ~(mask);
            }
            if (combined != 0) throw new ArgumentOutOfRangeException();
        }
    }
}
