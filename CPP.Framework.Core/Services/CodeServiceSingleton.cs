using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Abstract base class for all objects that provide services to applications using a singleton
    /// pattern (i.e. only one instance per application). This class provides a default
    /// implementation for the <see cref="ICodeServiceSingleton"/> interface, and is provided as
    /// the simplest means to implement a singleton service pattern for classes that have direct
    /// control over their inheritance hierarchy. However, for classes where this is not the case,
    /// they can still access the singleton service pattern by implementing the
    /// <see cref="ICodeServiceSingleton"/> interface directly, which will also provide access to
    /// the same level of functionality, albeit with slightly more coding required. Regardless of
    /// the mechanism chosen, it is never advisable to do both at the same time.
    /// </summary>
    /// <example>
    ///     <para>
    ///         The following example demonstrates how to define a singleton service that provides
    ///         an initialization and tear-down routine using the
    ///         <see cref="CodeServiceSingleton"/> class, as well as providing information for
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
    ///         internal class FibonacciService : CodeServiceSingleton, IFibonacciService
    ///         {
    ///             private static readonly double Phi1 = (1 + Math.Sqrt(5)) / 2.0;
    ///             private static readonly double Phi2 = (1 - Math.Sqrt(5)) / 2.0;
    /// 
    ///             [SingletonInstanceCleanupMethod]
    ///             protected override void CleanupInstance()
    ///             {
    ///                 Debug.WriteLine("CleanupInstance Was Called.");
    ///             }
    /// 
    ///             [SingletonInstanceStartupMethod]
    ///             protected override void StartupInstance()
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
    public class CodeServiceSingleton : CodeService, ICodeServiceSingleton
    {
        /// <summary>
        /// Called by the application framework to cleanup the current instance prior to it being 
        /// destroyed.
        /// </summary>
        [SingletonInstanceCleanupMethod]
        protected internal virtual void CleanupInstance() { }

        /// <summary>
        /// Called by the application framework to perform any initialization tasks when the 
        /// instance is being created.
        /// </summary>
        [SingletonInstanceStartupMethod]
        protected internal virtual void StartupInstance() { }
    }
}
