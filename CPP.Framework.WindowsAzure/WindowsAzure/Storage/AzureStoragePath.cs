using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Helper class used to manage path strings to Azure Storage blobs.
    /// </summary>
    public static class AzureStoragePath
    {
        #region CustomValidator Class Declaration

        private sealed class CustomValidator : ICustomArgumentValidator<string>
        {
            /// <summary>
            /// Called by the <see cref="ArgumentValidator"/> class to perform custom validation of an
            /// argument value;
            /// </summary>
            /// <param name="paramName">The name of the argument in the parameter list.</param>
            /// <param name="paramValue">The value of the argument being validated.</param>
            public void ValidateArgument(string paramName, string paramValue)
            {
                CheckInvalidPathChars(paramValue, paramName);
            }
        }

        #endregion // CustomValidator Class Declaration

        private static readonly CustomValidator _PathValidator = new CustomValidator();
        private static readonly Regex ContainerFormat = new Regex(@"^[a-z0-9](?:[a-z0-9-]){2,62}$", RegexOptions.Compiled);
        /***
         * NOTE : This expression tests for nearly all of the characters that are considered valid
         * within a URL (according to RFC3986), *except* for the ':' character, which has been
         * explicitly excluded since this class only every deals with the path portion of the URL,
         * and would therefore not be valid unless it was already encoded anyway (i.e. "%3A").
         */
        private static readonly Regex UrlReserveChars = new Regex(@"[^A-Za-z0-9\-_\./\\~?#\[\]@!$&'()*+,;=% ]", RegexOptions.Compiled);

        /// <summary>
        /// The default character used to delimit an Azure Storage path.
        /// </summary>
        public const char SeparatorChar = '/';

        /// <summary>
        /// Verifies whether or not there are invalid characters in an Azure Storage path.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <param name="paramName">The name of the parameter to use for the exception, if needed.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> contains invalid characters.</exception>
        private static void CheckInvalidPathChars(string path, string paramName)
        {
            if (AzureStoragePath.HasInvalidPathChars(path))
            {
                throw new ArgumentException(ErrorStrings.InvalidAzurePathCharacters, paramName);
            }
        }

        /// <summary>
        /// Combines an array of strings into a path.
        /// </summary>
        /// <param name="paths">The path strings to combine.</param>
        /// <returns>The combined paths.</returns>
        public static string Combine(params string[] paths)
        {
            const string ParamName = "paths";
            ArgumentValidator.ValidateNotNull(() => paths);

            var buffer = new StringBuilder();
            for (var i = 0; i < paths.Length; i++)
            {
                if (paths[i] == null)
                {
                    throw new ArgumentNullException(ParamName);
                }
                if (paths[i].Length == 0) continue;

                CheckInvalidPathChars(paths[i], ParamName);
                var partial = paths[i].Trim(SeparatorChar);
                
                if (buffer.Length == 0)
                {
                    buffer.Append(partial);
                }
                else
                {
                    buffer.AppendFormat("{0}{1}", SeparatorChar, partial);
                }
            }
            return buffer.ToString();
        }

        /// <summary>
        /// Combines twu strings into a path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combined paths.</returns>
        public static string Combine(string path1, string path2)
        {
            ArgumentValidator.ValidateNotNull(() => path1);
            ArgumentValidator.ValidateNotNull(() => path2);
            Contract.EndContractBlock();
            ArgumentValidator.ValidateCustom(() => path1, _PathValidator);
            ArgumentValidator.ValidateCustom(() => path2, _PathValidator);
            return CombineNoChecks(path1, path2);
        }

        /// <summary>
        /// Combines three strings into a path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <param name="path3">The third path to combine.</param>
        /// <returns>The combined paths.</returns>
        public static string Combine(string path1, string path2, string path3)
        {
            ArgumentValidator.ValidateNotNull(() => path1);
            ArgumentValidator.ValidateNotNull(() => path2);
            ArgumentValidator.ValidateNotNull(() => path3);
            Contract.EndContractBlock();
            ArgumentValidator.ValidateCustom(() => path1, _PathValidator);
            ArgumentValidator.ValidateCustom(() => path2, _PathValidator);
            ArgumentValidator.ValidateCustom(() => path3, _PathValidator);
            return CombineNoChecks(CombineNoChecks(path1, path2), path3);
        }

        /// <summary>
        /// Combines four strings into a path.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <param name="path3">The third path to combine.</param>
        /// <param name="path4">The fourth path to combine.</param>
        /// <returns>The combined paths.</returns>
        public static string Combine(string path1, string path2, string path3, string path4)
        {
            ArgumentValidator.ValidateNotNull(() => path1);
            ArgumentValidator.ValidateNotNull(() => path2);
            ArgumentValidator.ValidateNotNull(() => path3);
            ArgumentValidator.ValidateNotNull(() => path4);
            Contract.EndContractBlock();
            ArgumentValidator.ValidateCustom(() => path1, _PathValidator);
            ArgumentValidator.ValidateCustom(() => path2, _PathValidator);
            ArgumentValidator.ValidateCustom(() => path3, _PathValidator);
            ArgumentValidator.ValidateCustom(() => path4, _PathValidator);
            return CombineNoChecks(CombineNoChecks(CombineNoChecks(path1, path2), path3), path4);
        }

        /// <summary>
        /// Combines two paths without checking for invalid characters.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The combined paths.</returns>
        private static string CombineNoChecks(string path1, string path2)
        {
            if (path2.Length == 0) return path1;
            if (path1.Length == 1) return path2;

            path1 = path1.Trim(SeparatorChar);
            path2 = path2.Trim(SeparatorChar);

            return path1 + SeparatorChar + path2;
        }

        /// <summary>
        /// Parses the container name from a fully qualified Azure storage path.
        /// </summary>
        /// <param name="path">The path string to parse.</param>
        /// <returns>The container name.</returns>
        public static string GetContainerName(string path)
        {
            ArgumentValidator.ValidateNotNull(() => path);
            if (path.Length != 0)
            {
                var index = path.IndexOf(SeparatorChar);
                if (index <= -1) index = path.Length;
                return path.Substring(0, index).ToLower();
            }
            return String.Empty;
        }

        /// <summary>
        /// Parses the file name of the blob from a fully qualified Azure Storage path.
        /// </summary>
        /// <param name="path">The path string to parse.</param>
        /// <returns>The blob file name.</returns>
        public static string GetBlobFileName(string path)
        {
            ArgumentValidator.ValidateNotNull(() => path);
            if (path.Length != 0)
            {
                var index = path.LastIndexOf(SeparatorChar);
                return ((index == -1) ? String.Empty : path.Substring(index + 1));
            }
            return String.Empty;
        }

        /// <summary>
        /// Parses the path to the blob from a fully qualified Azure Storage path.
        /// </summary>
        /// <param name="path">The path string to parse.</param>
        /// <returns>The blob file path.</returns>
        public static string GetBlobFilePath(string path)
        {
            ArgumentValidator.ValidateNotNull(() => path);
            if (path.Length != 0)
            {
                var index = path.IndexOf(SeparatorChar);
                return ((index == -1) ? String.Empty : path.Substring(index + 1));
            }
            return String.Empty;
        }

        /// <summary>
        /// Checks whether or not a fully qualified or partial Azure Storage path contains invalid
        /// characters.
        /// </summary>
        /// <param name="path">The path string to validate.</param>
        /// <returns>True if <paramref name="path"/> contains invalid characters; otherwise, false.</returns>
        public static bool HasInvalidPathChars(string path)
        {
            /***
             * We are disabling this for now, because it trips up on accented and international
             * characters, which causes problems for things like accessing blobs in storage. We
             * will need to revisit this validation later so that we can do it better, but for now
             * we'll just have to let it slide because it's causing more problems than it's fixing.
             */
            return false; // (UrlReserveChars.IsMatch(path));
        }

        /// <summary>
        /// Checks whether or not the container name in a fully qualified Azure Storage path is
        /// valid.
        /// </summary>
        /// <param name="path">The path string to validate.</param>
        /// <returns>True if the container name in <paramref name="path"/> is valid; otherwise, false.</returns>
        public static bool IsContainerNameValid(string path)
        {
            var container = AzureStoragePath.GetContainerName(path);
            return (ContainerFormat.IsMatch(container) && (container.IndexOf(@"--", StringComparison.OrdinalIgnoreCase) == -1));
        }

        /// <summary>
        /// Removes invalid characters from an Azure Storage path.
        /// </summary>
        /// <param name="path">The path that contains the characters to remove.</param>
        /// <returns>The cleaned path.</returns>
        public static string RemoveInvalidChars(string path)
        {
            return UrlReserveChars.Replace(path, String.Empty);
        }
    }
}
