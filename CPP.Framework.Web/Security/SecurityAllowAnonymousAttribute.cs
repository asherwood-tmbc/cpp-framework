using System;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Applied to a controller class or method to direct the code to skip any authorization checks
    /// for the current principal.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SecurityAllowAnonymousAttribute : Attribute { }
}
