namespace CPP.Framework.Services
{
    /// <summary>
    /// Provides a container used to manages the lifetime of a service provider instance.
    /// </summary>
    /// <typeparam name="TProvider">The type of the service provider class.</typeparam>
    internal abstract class CodeServiceContainer<TProvider> : CodeServiceContainer where TProvider : ICodeService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeServiceContainer{TProvider}"/> 
        /// class.
        /// </summary>
        /// <param name="activator">
        /// The <see cref="CodeServiceActivator{TProvider}"/> object that is used to create new 
        /// instances of the service.
        /// </param>
        protected CodeServiceContainer(CodeServiceActivator<TProvider> activator) : base(activator) { }
    }
}
