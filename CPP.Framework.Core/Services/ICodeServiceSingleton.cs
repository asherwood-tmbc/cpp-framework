using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Abstract interface that is implemented by service classes that should be registered as a
    /// singleton, but cannot derive from the <see cref="CodeServiceSingleton"/> class. Please note
    /// that this interface is provided for service classes that cannot derive from the
    /// <see cref="CodeServiceSingleton"/> class for whatever reason, which is the preferred way to
    /// define service classes. Regardless, this interface will still allow access to the same
    /// level of functionality as <see cref="CodeServiceSingleton"/>, albeit with slightly more
    /// coding required. However, it is never advisable to implement
    /// <see cref="ICodeServiceSingleton"/> and derive from <see cref="CodeServiceSingleton"/> at
    /// the same time.
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
    public interface ICodeServiceSingleton : ICodeService { }
}
