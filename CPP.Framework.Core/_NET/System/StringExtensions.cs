using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using CPP.Framework;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Extension methods for the system <see cref="string"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class StringExtensions
    {
        /// <summary>
        /// The name of the format code capture group in the <see cref="FormatSpecifierPattern"/>
        /// expression.
        /// </summary>
        private const string FormatCodeGroupName = "code";

        /// <summary>
        /// The name of the argument index in the format code for the
        /// <see cref="FormatArgumentsPattern"/> expression.
        /// </summary>
        private const string ParamsIndexGroupName = "index";

        /// <summary>
        /// The <see cref="Regex"/> pattern for valid format specifier arguments.
        /// </summary>
        private static readonly Regex FormatArgumentsPattern = new Regex(@"(?<" + ParamsIndexGroupName + @">\d+?)(?:\:.+)?");

        /// <summary>
        /// The <see cref="Regex"/> pattern for format specifiers in a format string.
        /// </summary>
        private static readonly Regex FormatSpecifierPattern = new Regex(@"{(?<" + FormatCodeGroupName + @">[^}]+?)}", (RegexOptions.Compiled | RegexOptions.IgnoreCase));

        /// <summary>
        /// Modifies a format string by escaping any character sequences that would be interpreted
        /// as an invalid format specifier, either because the index is outside the bounds of the
        /// arguments array, or because the text within the curly braces is not an index as all.
        /// </summary>
        /// <param name="format">The format string to modify.</param>
        /// <param name="args">The available format arguments.</param>
        /// <returns>A string that contains the modified value.</returns>
        private static string CleanupFormatString(string format, ParamsArray args)
        {
            if (format != null)
            {
                string Evaluator(Match found)
                {
                    var match = FormatArgumentsPattern.Match(found.Groups[FormatCodeGroupName].Value);
                    if (match.Success && int.TryParse(match.Groups[ParamsIndexGroupName].Value, out var index))
                    {
                        if ((index >= 0) && (index < args.Length) && (args[index] != null))
                        {
                            return found.Value; // the specifier is valid, so return the matched value
                        }
                    }

                    // if we get to this point, then the specifier is not valid, either because the
                    // index is bad, the item at the index is null, or because the matched value is
                    // not a format specifier at all (e.g. embedded curly braces within a file name
                    // in an exception stack trace). so, just wrap a second set of braces around it
                    // to escape the value so that it won't crash with the call to String.Format().
                    return $"{{{found.Value}}}";
                }
                format = FormatSpecifierPattern.Replace(format, Evaluator);
            }
            return format;
        }

        /// <summary>
        /// Safely checks the contents of a string to see if it contains another string value, 
        /// regardless of whether or not the references are null.
        /// </summary>
        /// <param name="source">The source string to search.</param>
        /// <param name="value">The value to search for within <paramref name="source"/>.</param>
        /// <returns>True if <paramref name="source"/> contains <paramref name="value"/>; otherwise false. This method also returns false if <paramref name="source"/> or <paramref name="value"/> are null references.</returns>
        public static bool SafeContains(this string source, string value)
        {
            return source.SafeContains(value, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Safely checks the contents of a string to see if it contains another string value, 
        /// regardless of whether or not the references are null.
        /// </summary>
        /// <param name="source">The source string to search.</param>
        /// <param name="value">The value to search for within <paramref name="source"/>.</param>
        /// <param name="comparisonType">The type of comparison to use when searching <paramref name="source"/> for <paramref name="value"/>.</param>
        /// <returns>True if <paramref name="source"/> contains <paramref name="value"/>; otherwise false. This method also returns false if <paramref name="source"/> or <paramref name="value"/> are null references.</returns>
        public static bool SafeContains(this string source, string value, StringComparison comparisonType)
        {
            return ((source != null) && (value != null) && (source.IndexOf(value, comparisonType) >= 0));
        }

        /// <summary>
        /// Safely checks the contents of a string to see if it ends with another string value, 
        /// regardless of whether or not the references are null.
        /// </summary>
        /// <param name="source">The source string to search.</param>
        /// <param name="value">The value to search for within <paramref name="source"/>.</param>
        /// <returns>True if <paramref name="source"/> contains <paramref name="value"/>; otherwise false. This method also returns false if <paramref name="source"/> or <paramref name="value"/> are null references.</returns>
        public static bool SafeEndsWith(this string source, string value)
        {
            return source.SafeEndsWith(value, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Safely checks the contents of a string to see if it ends with another string value, 
        /// regardless of whether or not the references are null.
        /// </summary>
        /// <param name="source">The source string to search.</param>
        /// <param name="value">The value to search for within <paramref name="source"/>.</param>
        /// <param name="comparisonType">The type of comparison to use when searching <paramref name="source"/> for <paramref name="value"/>.</param>
        /// <returns>True if <paramref name="source"/> contains <paramref name="value"/>; otherwise false. This method also returns false if <paramref name="source"/> or <paramref name="value"/> are null references.</returns>
        public static bool SafeEndsWith(this string source, string value, StringComparison comparisonType)
        {
            return ((source != null) && (value != null) && (source.EndsWith(value, comparisonType)));
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. However, unlike the
        /// <see cref="string.Format(string,object[])"/> method, this method escapes and ignores
        /// any invalid format specifiers embedding in the composite string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="provider">
        ///     An object that supplies culture-specific formatting information.
        /// </param>
        /// <param name="arg0">The first object to format.</param>
        /// <returns>
        /// A copy of <paramref name="format" /> in which the format items have been replaced by
        /// the string representation of <paramref name="arg0" />.
        /// </returns>
        public static string SafeFormatWith(this string format, IFormatProvider provider, object arg0)
        {
            format = CleanupFormatString(format, new ParamsArray(arg0));
            if ((provider != null) && (format != null))
            {
                format = string.Format(provider, format, arg0);
            }
            return format;
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. However, unlike the
        /// <see cref="string.Format(string,object[])"/> method, this method escapes and ignores
        /// any invalid format specifiers embedding in the composite string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="provider">
        ///     An object that supplies culture-specific formatting information.
        /// </param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <returns>
        /// A copy of <paramref name="format" /> in which the format items have been replaced by
        /// the string representation of <paramref name="arg0" /> and <paramref name="arg1" />.
        /// </returns>
        public static string SafeFormatWith(this string format, IFormatProvider provider, object arg0, object arg1)
        {
            format = CleanupFormatString(format, new ParamsArray(arg0, arg1));
            if ((provider != null) && (format != null))
            {
                format = string.Format(provider, format, arg0, arg1);
            }
            return format;
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. However, unlike the
        /// <see cref="string.Format(string,object[])"/> method, this method escapes and ignores
        /// any invalid format specifiers embedding in the composite string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="provider">
        ///     An object that supplies culture-specific formatting information.
        /// </param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <returns>
        /// A copy of <paramref name="format" /> in which the format items have been replaced by
        /// the string representation of <paramref name="arg0" />, <paramref name="arg1" />, and
        /// <paramref name="arg2" />.
        /// </returns>
        public static string SafeFormatWith(this string format, IFormatProvider provider, object arg0, object arg1, object arg2)
        {
            format = CleanupFormatString(format, new ParamsArray(arg0, arg1, arg2));
            if ((provider != null) && (format != null))
            {
                format = string.Format(provider, format, arg0, arg1, arg2);
            }
            return format;
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. However, unlike the
        /// <see cref="string.Format(string,object[])"/> method, this method escapes and ignores
        /// any invalid format specifiers embedding in the composite string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="provider">
        ///     An object that supplies culture-specific formatting information.
        /// </param>
        /// <param name="args">
        ///     An object array that contains zero or more objects to format.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="format" /> in which the format items have been replaced by
        /// the string representation of the corresponding objects in <paramref name="args" />.
        /// </returns>
        public static string SafeFormatWith(this string format, IFormatProvider provider, params object[] args)
        {
            format = CleanupFormatString(format, new ParamsArray(args));
            if ((provider != null) && (format != null) && (args != null))
            {
                format = string.Format(provider, format, args);
            }
            return format;
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. However, unlike the
        /// <see cref="string.Format(string,object[])"/> method, this method escapes and ignores
        /// any invalid format specifiers embedding in the composite string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <returns>
        /// A copy of <paramref name="format" /> in which the format items have been replaced by
        /// the string representation of <paramref name="arg0" />.
        /// </returns>
        public static string SafeFormatWith(this string format, object arg0)
        {
            format = CleanupFormatString(format, new ParamsArray(arg0));
            return ((format == null) ? null : string.Format(format, arg0));
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. However, unlike the
        /// <see cref="string.Format(string,object[])"/> method, this method escapes and ignores
        /// any invalid format specifiers embedding in the composite string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <returns>
        /// A copy of <paramref name="format" /> in which the format items have been replaced by
        /// the string representation of <paramref name="arg0" /> and <paramref name="arg1" />.
        /// </returns>
        public static string SafeFormatWith(this string format, object arg0, object arg1)
        {
            format = CleanupFormatString(format, new ParamsArray(arg0, arg1));
            return ((format == null) ? null : string.Format(format, arg0, arg1));
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. However, unlike the
        /// <see cref="string.Format(string,object[])"/> method, this method escapes and ignores
        /// any invalid format specifiers embedding in the composite string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <returns>
        /// A copy of <paramref name="format" /> in which the format items have been replaced by
        /// the string representation of <paramref name="arg0" />, <paramref name="arg1" />, and
        /// <paramref name="arg2" />.
        /// </returns>
        public static string SafeFormatWith(this string format, object arg0, object arg1, object arg2)
        {
            format = CleanupFormatString(format, new ParamsArray(arg0, arg1, arg2));
            return ((format == null) ? null : string.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        /// Replaces the format item in a specified string with the string representation of a
        /// corresponding object in a specified array. However, unlike the
        /// <see cref="string.Format(string,object[])"/> method, this method escapes and ignores
        /// any invalid format specifiers embedding in the composite string.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        /// An object array that contains zero or more objects to format.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="format" /> in which the format items have been replaced by
        /// the string representation of the corresponding objects in <paramref name="args" />.
        /// </returns>
        public static string SafeFormatWith(this string format, params object[] args)
        {
            format = CleanupFormatString(format, new ParamsArray(args));
            return ((format == null) ? null : string.Format(format, args));
        }

        /// <summary>
        /// Safely removes one or more characters from the contents of a string, regardless of 
        /// whether or not the reference is null.
        /// </summary>
        /// <param name="source">The source string to modify.</param>
        /// <param name="char1">The first character to remove.</param>
        /// <returns>The modified string.</returns>
        public static string SafeRemove(this string source, char char1)
        {
            if (source != null)
            {
                var buffer = source.Where(ch => (ch != char1));
                source = new string(buffer.ToArray());
            }
            return source;
        }

        /// <summary>
        /// Safely removes one or more characters from the contents of a string, regardless of 
        /// whether or not the reference is null.
        /// </summary>
        /// <param name="source">The source string to modify.</param>
        /// <param name="char1">The first character to remove.</param>
        /// <param name="char2">The second character to remove.</param>
        /// <returns>The modified string.</returns>
        public static string SafeRemove(this string source, char char1, char char2)
        {
            if (source != null)
            {
                var buffer = source.Where(ch => (ch != char1) && (ch != char2));
                source = new string(buffer.ToArray());
            }
            return source;
        }

        /// <summary>
        /// Safely removes one or more characters from the contents of a string, regardless of 
        /// whether or not the reference is null.
        /// </summary>
        /// <param name="source">The source string to modify.</param>
        /// <param name="char1">The first character to remove.</param>
        /// <param name="char2">The second character to remove.</param>
        /// <param name="char3">The fourth character to remove.</param>
        /// <returns>The modified string.</returns>
        public static string SafeRemove(this string source, char char1, char char2, char char3)
        {
            if (source != null)
            {
                var buffer = source.Where(ch => (ch != char1) && (ch != char2) && (ch != char3));
                source = new string(buffer.ToArray());
            }
            return source;
        }

        /// <summary>
        /// Safely removes one or more characters from the contents of a string, regardless of 
        /// whether or not the reference is null.
        /// </summary>
        /// <param name="source">The source string to modify.</param>
        /// <param name="char1">The first character to remove.</param>
        /// <param name="char2">The second character to remove.</param>
        /// <param name="char3">The fourth character to remove.</param>
        /// <param name="char4">The fifth character to remove.</param>
        /// <returns>The modified string.</returns>
        public static string SafeRemove(this string source, char char1, char char2, char char3, char char4)
        {
            if (source != null)
            {
                var buffer = source.Where(ch => (ch != char1) && (ch != char2) && (ch != char3) && (ch != char4));
                source = new string(buffer.ToArray());
            }
            return source;
        }

        /// <summary>
        /// Safely removes one or more characters from the contents of a string, regardless of 
        /// whether or not the reference is null.
        /// </summary>
        /// <param name="source">The source string to modify.</param>
        /// <param name="chars">An optional list of one or more characters to remove from <paramref name="source"/>.</param>
        /// <returns>The modified string.</returns>
        public static string SafeRemove(this string source, params char[] chars)
        {
            var count = chars.Length;
            if (source != null)
            {
                switch (count)
                {
                    case 1:
                        {
                            source = source.SafeRemove(chars[0]);
                        }
                        break;
                    case 2:
                        {
                            source = source.SafeRemove(chars[0], chars[1]);
                        }
                        break;
                    case 3:
                        {
                            source = source.SafeRemove(chars[0], chars[1], chars[2]);
                        }
                        break;
                    case 4:
                        {
                            source = source.SafeRemove(chars[0], chars[1], chars[2], chars[3]);
                        }
                        break;
                    default:
                        {
                            var charset = new HashSet<char>(chars);
                            var buffer = source.Where(ch => (!charset.Contains(ch)));
                            source = new string(buffer.ToArray());
                        }
                        break;
                }
            }
            return source;
        }

        /// <summary>
        /// Safely checks the contents of a string to see if it starts with another string value, 
        /// regardless of whether or not the references are null.
        /// </summary>
        /// <param name="source">The source string to search.</param>
        /// <param name="value">The value to search for within <paramref name="source"/>.</param>
        /// <returns>True if <paramref name="source"/> contains <paramref name="value"/>; otherwise false. This method also returns false if <paramref name="source"/> or <paramref name="value"/> are null references.</returns>
        public static bool SafeStartsWith(this string source, string value)
        {
            return source.SafeStartsWith(value, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Safely checks the contents of a string to see if it starts with another string value, 
        /// regardless of whether or not the references are null.
        /// </summary>
        /// <param name="source">The source string to search.</param>
        /// <param name="value">The value to search for within <paramref name="source"/>.</param>
        /// <param name="comparisonType">The type of comparison to use when searching <paramref name="source"/> for <paramref name="value"/>.</param>
        /// <returns>True if <paramref name="source"/> contains <paramref name="value"/>; otherwise false. This method also returns false if <paramref name="source"/> or <paramref name="value"/> are null references.</returns>
        public static bool SafeStartsWith(this string source, string value, StringComparison comparisonType)
        {
            return ((source != null) && (value != null) && (source.StartsWith(value, comparisonType)));
        }

        /// <summary>
        /// Safely replace tokens matched by a regular expression with replacement parameters. If
        /// the regular expression contains capture groups, then the value for the first captured 
        /// group in each match will be used for the token value, otherwise the entire match will 
        /// be used.
        /// </summary>
        /// <param name="source">The input string to search for replacement tokens.</param>
        /// <param name="pattern">The regular expression pattern used to match the token values.</param>
        /// <param name="parameters">
        ///     <para>An <see cref="IDictionary{TKey,TValue}"/> object that contains replacement parameters, using the token for the key, and the value for the replacement.</para>
        ///     <para>-or-</para>
        ///     <para>An anonymous class that defines one or more replacement parameters, with the token as the property name, and the property value as the replacement.</para>
        /// </param>
        /// <returns>A copy of <paramref name="source"/>, with the matching tokens replaced.</returns>
        public static string SafeTokenReplace(this string source, Regex pattern, object parameters = null)
        {
            if (source != null)
            {
                ArgumentValidator.ValidateNotNull(() => pattern);

                var tokenValueMap = default(ReadOnlyDictionary<string, string>);
                if (parameters is IDictionary<string, string> dictionary)
                {
                    var collection = dictionary;
                    tokenValueMap = new ReadOnlyDictionary<string, string>(collection);
                }
                else if (parameters != null)
                {
                    var collection = parameters.GetType()
                        .GetProperties()
                        .ToDictionary(pi => pi.Name, pi => Convert.ToString(pi.GetValue(parameters)));
                    tokenValueMap = new ReadOnlyDictionary<string, string>(collection);
                }
                else tokenValueMap = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

                source = pattern.Replace(
                    source,
                    match =>
                        {
                            var token = match.Groups.Count >= 2 ? match.Groups[1].Value : match.Groups[0].Value;
                            if (!tokenValueMap.TryGetValue(token, out var value) || (value == null))
                            {
                                value = match.Groups[0].Value;
                            }
                            return value;
                        });
            }
            return source;
        }

        /// <summary>
        /// Safely converts the value of a string to lower case, regardless of whether or not the 
        /// reference is null.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <returns>The lower cased string, or null if <paramref name="source"/> is null.</returns>
        public static string SafeToLower(this string source)
        {
            return source?.ToLower();
        }

        /// <summary>
        /// Safely converts the value of a string to lower case, regardless of whether or not the 
        /// reference is null.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> object that provides culture-specific casing rules.</param>
        /// <returns>The lower cased string, or null if <paramref name="source"/> is null.</returns>
        public static string SafeToLower(this string source, CultureInfo culture)
        {
            return source?.ToLower(culture);
        }

        /// <summary>
        /// Safely converts the value of a string to lower case, regardless of whether or not the 
        /// reference is null.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <returns>The lower cased string, or null if <paramref name="source"/> is null.</returns>
        public static string SafeToLowerInvariant(this string source)
        {
            return source?.ToLowerInvariant();
        }

        /// <summary>
        /// Safely converts the value of a string to upper case, regardless of whether or not the 
        /// reference is null.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <returns>The lower cased string, or null if <paramref name="source"/> is null.</returns>
        public static string SafeToUpper(this string source)
        {
            return source?.ToUpper();
        }

        /// <summary>
        /// Safely converts the value of a string to upper case, regardless of whether or not the 
        /// reference is null.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> object that provides culture-specific casing rules.</param>
        /// <returns>The lower cased string, or null if <paramref name="source"/> is null.</returns>
        public static string SafeToUpper(this string source, CultureInfo culture)
        {
            return source?.ToUpper(culture);
        }

        /// <summary>
        /// Safely converts the value of a string to upper case, regardless of whether or not the 
        /// reference is null.
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <returns>The lower cased string, or null if <paramref name="source"/> is null.</returns>
        public static string SafeToUpperInvariant(this string source)
        {
            return source?.ToUpperInvariant();
        }

        /// <summary>
        /// Safely trims the value of a string, regardless of whether or not the reference is null.
        /// </summary>
        /// <param name="source">The string to trim.</param>
        /// <returns>The trimmed string, or null if <paramref name="source"/> is null.</returns>
        public static string SafeTrim(this string source)
        {
            return source?.Trim();
        }

        /// <summary>
        /// Safely trims the value of a string, regardless of whether or not the reference is null.
        /// </summary>
        /// <param name="source">The string to trim.</param>
        /// <param name="trimChars">An optional list of characters to trim. If this list is empty, then whitespace characters are removed instead.</param>
        /// <returns>The trimmed string, or null if <paramref name="source"/> is null.</returns>
        public static string SafeTrim(this string source, params char[] trimChars)
        {
            return source?.Trim(trimChars);
        }

        #region ParamsArray Class Declaration

        /// <summary>
        /// Helper class adapted from the .NET Framework libraries to speed up array access for the
        /// most comment string formatting scenarios (i.e. parameter arrays with 3 or fewer items).
        /// </summary>
        private sealed class ParamsArray
        {
            /// <summary>
            /// An empty object array.
            /// </summary>
            private static readonly object[] EmptyArgsArray = new object[0];

            /// <summary>
            /// A static object array with a single element.
            /// </summary>
            private static readonly object[] SingleArgArray = new object[1];

            /// <summary>
            /// A static object array with two elements.
            /// </summary>
            private static readonly object[] DoubleArgArray = new object[2];

            /// <summary>
            /// A static object array with three elements.
            /// </summary>
            private static readonly object[] ThreeArgsArray = new object[3];

            /// <summary>
            /// The first format argument value.
            /// </summary>
            private readonly object _arg0;

            /// <summary>
            /// The second format argument value.
            /// </summary>
            private readonly object _arg1;

            /// <summary>
            /// The third format argument value.
            /// </summary>
            private readonly object _arg2;

            /// <summary>
            /// The full argument array.
            /// </summary>
            private readonly object[] _args;

            /// <summary>
            /// Initializes a new instance of the <see cref="ParamsArray"/> class.
            /// </summary>
            /// <param name="arg0">The first argument to format.</param>
            internal ParamsArray(object arg0)
            {
                _arg0 = arg0;
                _arg1 = default(object);
                _arg2 = default(object);
                _args = SingleArgArray;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ParamsArray"/> class.
            /// </summary>
            /// <param name="arg0">The first argument to format.</param>
            /// <param name="arg1">The second argument to format.</param>
            internal ParamsArray(object arg0, object arg1)
            {
                _arg0 = arg0;
                _arg1 = arg1;
                _arg2 = default(object);
                _args = DoubleArgArray;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ParamsArray"/> class.
            /// </summary>
            /// <param name="arg0">The first argument to format.</param>
            /// <param name="arg1">The second argument to format.</param>
            /// <param name="arg2">The third argument to format.</param>
            internal ParamsArray(object arg0, object arg1, object arg2)
            {
                _arg0 = arg0;
                _arg1 = arg1;
                _arg2 = arg2;
                _args = ThreeArgsArray;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ParamsArray"/> class.
            /// </summary>
            /// <param name="args">The array of arguments to format.</param>
            internal ParamsArray(object[] args)
            {
                args = (args ?? EmptyArgsArray);
                var length = args.Length;
                _arg0 = ((length > 0) ? args[0] : default(object));
                _arg1 = ((length > 1) ? args[1] : default(object));
                _arg2 = ((length > 2) ? args[2] : default(object));
                _args = args;
            }

            /// <summary>
            /// Gets the length of the arguments array.
            /// </summary>
            internal int Length => _args.Length;

            /// <summary>
            /// Gets the value of a format argument at the given index.
            /// </summary>
            /// <param name="index">The index of the argument to format.</param>
            /// <returns>The object to format.</returns>
            internal object this[int index]
            {
                get
                {
                    if (index != 0)
                    {
                        return GetAtSlow(index);
                    }
                    return _arg0;
                }
            }

            /// <summary>
            /// Accesses any format arguments beyond the first one.
            /// </summary>
            /// <param name="index">The index of the argument to format.</param>
            /// <returns>The object to format.</returns>
            private object GetAtSlow(int index)
            {
                if (index == 1) return _arg1;
                if (index == 2) return _arg2;
                return _args[index];
            }
        }

        #endregion // ParamsArray Class Declaration
    }
}
