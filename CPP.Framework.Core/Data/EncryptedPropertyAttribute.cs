using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Data
{
    /// <summary>
    /// Marks a field or property as encrypted in the data source, requiring it to be decrypted 
    /// when being read, and encrypted when being saved.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [ExcludeFromCodeCoverage]
    public sealed class EncryptedPropertyAttribute : ConfidentialAttribute { }
}
