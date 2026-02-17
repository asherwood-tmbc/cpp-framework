using System;
using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Attribute that is applied to a parameterless instance method on a class that implements the
    /// <see cref="ICodeServiceSingleton"/> interface to indicate the routine that should be called
    /// when an instance of the service is being torn down. Please note that the usage of this
    /// attribute is entirely optional, and that the visibility of the method does not need to be
    /// public. However, this attribute should not be used with classes that derive from the
    /// <see cref="CodeServiceSingleton"/> class, as it already provides an overridable
    /// <see cref="CodeServiceSingleton.CleanupInstance"/> method for this purpose.
    /// </summary>
    /// <example>
    ///     <para>
    ///         The following example demonstrates how to define a singleton service that provides
    ///         an initialization and tear-down routine using the
    ///         <see cref="ICodeServiceSingleton"/> interface, as well as providing information for
    ///         automatic registration through the <see cref="ServiceLocator.RegisterAll(string[])"/>
    ///         method using the <see cref="ServiceRegistrationAttribute"/> attribute.
    ///     </para>
    ///     <code language="c#">
    ///     <![CDATA[
    ///         using System;
    ///         using System.Diagnostics;
    /// 
    ///         using CPP.Framework.Services;
    ///
    ///         public interface IFibonacciService
    ///         {
    ///             double GetValueAt(int pos);
    ///         }
    /// 
    ///         [ServiceRegistration(typeof(IFibonacciService))]
    ///         internal class FibonacciService : ICodeServiceSingleton, IFibonacciService
    ///         {
    ///             private static readonly double Phi1 = (1 + Math.Sqrt(5)) / 2.0;
    ///             private static readonly double Phi2 = (1 - Math.Sqrt(5)) / 2.0;
    /// 
    ///             [SingletonInstanceCleanupMethod]
    ///             protected void CleanupInstance()
    ///             {
    ///                 Debug.WriteLine("CleanupInstance Was Called.");
    ///             }
    /// 
    ///             [SingletonInstanceStartupMethod]
    ///             protected void StartupInstance()
    ///             {
    ///                 Debug.WriteLine("StartupInstance Was Called.");
    ///             }
    /// 
    ///             public double GetValueAt(int pos)
    ///             {
    ///                 return (Math.Pow(Phi1, pos) - Math.Pow(Phi2, pos)) / Math.Sqrt(5);
    ///             }
    ///         }
    ///     ]]>
    ///     </code>
    ///     <para>
    ///         You can then call the service method using the following code:
    ///     </para>
    ///     <code language="c#">
    ///     <![CDATA[
    ///         ServiceLocator.RegisterAll();   // somewhere in your application startup
    /// 
    ///         ...
    /// 
    ///         Debug.WriteLine($"10 = {ServiceLocator.GetInstance<IFibonacciService>().GetValueAt(10)}");
    ///         Debug.WriteLine($"20 = {ServiceLocator.GetInstance<IFibonacciService>().GetValueAt(20)}");
    /// 
    ///         ...
    ///  
    ///         ServiceLocator.Unload();        // somewhere in your application shutdown
    ///     ]]>
    ///     </code>
    ///     <para>
    ///         Which should produce the following output in the debug window:
    ///     </para>
    ///     <code language="none">
    ///     <![CDATA[
    ///         StartupInstance Was Called.
    ///         10 = 55
    ///         20 = 6765
    ///         CleanupInstance Was Called.
    ///     ]]>
    ///     </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SingletonInstanceCleanupMethodAttribute : Attribute { }
}
