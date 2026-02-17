using System.Diagnostics.CodeAnalysis;
using CPP.Framework.DependencyInjection.Resolvers;
using Microsoft.Practices.Unity;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Helper class used to provider <see cref="ServiceResolver"/> definition to the
    /// <see cref="ServiceLocator"/> when it is resolving the parameters to a constructor during
    /// constructor dependency injection (i.e. when a type is being constructed).
    /// </summary>
    /// <example>
    ///     <para>
    ///         This example demonstrates how to use the <see cref="WithResolver"/> class to inject
    ///         a specific object instance into a constructor based on the type when resolving a
    ///         object instance through the <see cref="ServiceLocator"/>.
    ///     </para>
    ///     <code language="c#">
    ///     <![CDATA[
    ///         using System;
    ///         using System.Diagnostics;
    /// 
    ///         using CPP.Framework.DependencyInjection;
    ///         using CPP.Framework.DependencyInjection.Resolvers;
    /// 
    ///         public interface IObjectFactory
    ///         {
    ///             string GetFactoryName { get; }
    ///         }
    /// 
    ///         internal class MyObjectFactory : IObjectFactory
    ///         {
    ///             static MyObjectFactory()
    ///             {
    ///                 MyObjectFactory.Instance = new MyObjectFactory();
    ///             }
    /// 
    ///             public string GetFactoryName
    ///             {
    ///                 get { return "MyObjectFactory"; }
    ///             }
    /// 
    ///             public static MyObjectFactory Instance { get; }
    ///         }
    /// 
    ///         internal class SampleConsumer
    ///         {
    ///             private readonly IObjectFactory _factory;
    /// 
    ///             [ServiceLocatorConstructor]
    ///             protected SampleConsumer(IObjectFactory factory)
    ///             {
    ///                 _factory = factory;
    ///             }
    /// 
    ///             public void Consume()
    ///             {
    ///                 System.Diagnostics.Debug.WriteLine($"Consuming from "{_factory.GetFactoryName()}");
    ///             }
    ///         }
    ///     ]]>
    ///     </code>
    ///     <para>
    ///         You can then call the service method using the following code:
    ///     </para>
    ///     <code language="c#">
    ///     <![CDATA[
    ///         var service = default(SampleConsumer);
    ///         service = ServiceLocator.GetInstance<SampleConsumer>(
    ///             WithResolver.ForInstance<IObjectFactory>(MyObjectFactory.Instance));
    ///         service.Consume();
    ///     ]]>
    ///     </code>
    ///     <para>
    ///         Which should produce the following output in the debug window:
    ///     </para>
    ///     <code language="none">
    ///     <![CDATA[
    ///         Consuming from "MyObjectFactory"
    ///     ]]>
    ///     </code>
    /// </example>
    public static class WithResolver
    {
        /// <summary>
        /// Creates a <see cref="ServiceResolver"/> to a given instance for an interface type.
        /// </summary>
        /// <typeparam name="TService">The type of the interface.</typeparam>
        /// <param name="resolvedTo">
        /// The instance to return when resolving references of <typeparamref name="TService"/>.
        /// </param>
        /// <returns>A <see cref="ServiceResolver"/> instance.</returns>
        [ExcludeFromCodeCoverage]
        public static ServiceResolver ForInstance<TService>(TService resolvedTo) => new DependencyResolver<TService>(resolvedTo);

        /// <summary>
        /// Creates a new <see cref="ParameterResolver"/> for a given named constructor parameter.
        /// </summary>
        /// <param name="targetName">The name of the target parameter to inject.</param>
        /// <param name="resolvesTo">
        /// The value to inject for any parameters with a name matching <paramref name="targetName"/>
        /// in the constructor when resolving the service instance.
        /// </param>
        /// <returns>A <see cref="ServiceResolver"/> instance.</returns>
        [ExcludeFromCodeCoverage]
        public static ServiceResolver ForParameter(string targetName, object resolvesTo) => new ParameterResolver(targetName, resolvesTo);

        /// <summary>
        /// Creates a new <see cref="ServiceResolver"/> that can be used to resolve any references
        /// to an <see cref="IUnityContainer"/> to the instance that is in currently being used by
        /// the <see cref="ServiceLocator"/>.
        /// </summary>
        /// <returns>A <see cref="ServiceResolver"/> instance.</returns>
        [ExcludeFromCodeCoverage]
        public static ServiceResolver ForUnityContainer() => UnityContainerResolver.Instance;
    }
}
