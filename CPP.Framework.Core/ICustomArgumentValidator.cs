namespace CPP.Framework
{
    /// <summary>
    /// Abstract base interface for classes that provider custom argument validation through the
    /// <see cref="ArgumentValidator"/> class.
    /// </summary>
    /// <typeparam name="TValue">The type of the argument to validate.</typeparam>
    public interface ICustomArgumentValidator<in TValue>
    {
        /// <summary>
        /// Called by the <see cref="ArgumentValidator"/> class to perform custom validation of an
        /// argument value;
        /// </summary>
        /// <param name="paramName">The name of the argument in the parameter list.</param>
        /// <param name="paramValue">The value of the argument being validated.</param>
        void ValidateArgument(string paramName, TValue paramValue);
    }
}
