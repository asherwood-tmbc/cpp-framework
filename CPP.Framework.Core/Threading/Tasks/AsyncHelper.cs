using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace CPP.Framework.Threading.Tasks
{
    /// <summary>
    /// Utility class used to execute async/await methods from synchronous code and return the 
    /// results. This class is borrowed from the one used internally by various Microsoft libraries,
    /// and should prevent deadlock issues in certain runtime environments (like ASP.NET) that may
    /// use a custom <see cref="SynchronizationContext"/> that isn't based on dedicated background
    /// threads from a thread pool to execute each task.
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _Factory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

        /// <summary>
        /// Executes an async/await function, and waits for the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="function">The async/await function to execute.</param>
        /// <returns>The return value of <paramref name="function"/>.</returns>
        [UsedImplicitly]
        public static TResult RunSync<TResult>(Func<Task<TResult>> function)
        {
            ArgumentValidator.ValidateNotNull(() => function);
            var result = _Factory
                .StartNew(function)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
            return result;
        }

        /// <summary>
        /// Executes an async/await function, and waits for the result.
        /// </summary>
        /// <param name="function">The async/await function to execute.</param>
        [UsedImplicitly]
        public static void RunSync(Func<Task> function)
        {
            ArgumentValidator.ValidateNotNull(() => function);
            _Factory
                .StartNew(function)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}
