// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    /// <summary>
    /// Helpers for safely using Task libraries
    /// (imported verbatim from the .NET Framework 4.5 Libraries)
    /// </summary>
    internal static class TaskHelpers
    {
        private static readonly Task<object> _CompletedTaskReturningNull = Task.FromResult<object>(null);
        private static readonly Task _DefaultCompleted = Task.FromResult<AsyncVoid>(default(AsyncVoid));

        /// <summary>
        /// Returns a canceled Task.
        /// The task is completed, IsCanceled = True, IsFaulted = False.
        /// </summary>
        /// <returns>The cancelled task object.</returns>
        internal static Task Canceled() => CancelCache<AsyncVoid>.Canceled;

        /// <summary>
        /// Returns a canceled Task of the given type.
        /// The task is completed, IsCanceled = True, IsFaulted = False.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>The cancelled task object.</returns>
        internal static Task<TResult> Canceled<TResult>() => CancelCache<TResult>.Canceled;

        /// <summary>
        /// Returns a completed task that has no result. 
        /// </summary>
        /// <returns>The completed task object.</returns>
        internal static Task Completed() => _DefaultCompleted;

        /// <summary>
        /// Returns an error task.
        /// The task is Completed, IsCanceled = False, IsFaulted = True
        /// </summary>
        /// <param name="exception">The exception for the error.</param>
        /// <returns>The error task object.</returns>
        internal static Task FromError(Exception exception) => FromError<AsyncVoid>(exception);

        /// <summary>
        /// Returns an error task of the given type.
        /// The task is Completed, IsCanceled = False, IsFaulted = True
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="exception">The exception for the error.</param>
        /// <returns>The error task object.</returns>
        internal static Task<TResult> FromError<TResult>(Exception exception)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        /// <summary>
        /// Returns a completed task with a null result.
        /// </summary>
        /// <returns>The completed task object.</returns>
        internal static Task<object> NullResult() => _CompletedTaskReturningNull;

        /// <summary>
        /// Used as the T in a "conversion" of a Task into a Task{T}
        /// </summary>
        private struct AsyncVoid { }

        /// <summary>
        /// This class is a convenient cache for per-type cancelled tasks
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        private static class CancelCache<TResult>
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static readonly Task<TResult> Canceled = GetCancelledTask();

            /// <summary>
            /// Returns a canceled Task.
            /// The task is completed, IsCanceled = True, IsFaulted = False.
            /// </summary>
            /// <returns>The cancelled task object.</returns>
            private static Task<TResult> GetCancelledTask()
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
                tcs.SetCanceled();
                return tcs.Task;
            }
        }
    }
}
