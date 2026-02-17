using System;

namespace CPP.Framework.Data
{
    /// <summary>
    /// Applied to a field or property to indicate that it is confidential, and should not be 
    /// exposed outside the application as a plain text value (e.g. as in log messages).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ConfidentialAttribute : Attribute { }
}
