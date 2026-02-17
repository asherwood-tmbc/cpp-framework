using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Applied to a class or an interface to explicitly provide the name for the associated Azure 
    /// Storage table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [ExcludeFromCodeCoverage]
    public class AzureTableNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="tableName">The name of the storage table associated with the class.</param>
        public AzureTableNameAttribute(string tableName)
        {
            this.TableName = tableName;
        }

        /// <summary>
        /// Gets the name of the storage table associated with the class.
        /// </summary>
        public string TableName { get; private set; }
    }
}
