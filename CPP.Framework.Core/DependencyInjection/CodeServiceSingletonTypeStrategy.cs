using CPP.Framework.Security;
using CPP.Framework.Services;

using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Build strategy used to ensure types that derive from <see cref="CodeServiceSingleton"/> are
    /// not created multiple times in the case where a service is using automatic registration, but
    /// was resolved by the container due to constructor injection by another class before the
    /// <see cref="CodeServiceProvider"/> has a chance to register the service, leading to multiple
    /// instances of what was supposed to be a singleton (e.g. like
    /// <see cref="SecurityAuthorizationContext"/> with <see cref="SecurityAuthorizationManager"/>
    /// during the call to <see cref="SecurityAuthorizationContext.Create()"/>).
    /// </summary>
    internal class CodeServiceSingletonTypeStrategy : BuilderStrategy
    {
        private readonly ServiceLocatorExtension _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeServiceSingletonTypeStrategy"/> class.
        /// </summary>
        /// <param name="parent">
        /// The <see cref="ServiceLocatorExtension"/> that created the current instance.
        /// </param>
        internal CodeServiceSingletonTypeStrategy(ServiceLocatorExtension parent)
        {
            ArgumentValidator.ValidateNotNull(() => parent);
            _parent = parent;
        }

        /// <summary>
        /// Called during the chain of responsibility for a build operation. The
        /// PreBuildUp method is called when the chain is being executed in the
        /// forward direction.
        /// </summary>
        /// <param name="context">Context of the build operation.</param>
        public override void PreBuildUp(IBuilderContext context)
        {
            ArgumentValidator.ValidateNotNull(() => context);
            if (context.BuildKey == null) return;
            if (context.BuildKey.Type == null) return;
            if (typeof(ICodeServiceSingleton).IsAssignableFrom(context.BuildKey.Type))
            {
                var policy = context.PersistentPolicies.GetNoDefault<ILifetimePolicy>(context.BuildKey, false);
                if (policy == null)
                {
                    policy = new CodeServiceSingletonLifetimeManager();
                }
                context.PersistentPolicies.Set(policy, context.BuildKey);
            }
            base.PreBuildUp(context);
        }
    }
}
