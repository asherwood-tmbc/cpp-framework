using System;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Applied to a static method within a service class that can be used to create registered
    /// instances of the containing class.
    /// </summary>
    /// <remarks>
    /// The method name and scope can be anything (as long as it's static), however the signature 
    /// must take a single parameter of type <see cref="string"/>, which is the passed the name of
    /// the registration for the service being created (or null for default registrations). In 
    /// addition, the return value can be either the service interface type, or the implementation 
    /// type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CreateServiceInstanceAttribute : Attribute { }
}
