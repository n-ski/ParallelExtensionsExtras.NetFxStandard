//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: TaskFactoryExtensions_From.cs
//
//--------------------------------------------------------------------------

namespace System.Threading.Tasks
{
    /// <summary>Extensions for <see cref="TaskFactory"/>.</summary>
    public static partial class TaskFactoryExtensions
    {
        #region TaskFactory
        /// <summary>Creates a <see cref="Task"/> that has completed in the <see cref="TaskStatus.Faulted"/> state with the specified exception.</summary>
        /// <param name="factory">The target <see cref="TaskFactory"/>.</param>
        /// <param name="exception">The exception with which the <see cref="Task"/> should fault.</param>
        /// <returns>The completed <see cref="Task"/>.</returns>
        public static Task FromException(this TaskFactory factory, Exception exception)
        {
            var tcs = new TaskCompletionSource<object>(factory.CreationOptions);
            tcs.SetException(exception);
            return tcs.Task;
        }

        /// <summary>Creates a <see cref="Task"/> that has completed in the <see cref="TaskStatus.Faulted"/> state with the specified exception.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new <see cref="Task"/>.</typeparam>
        /// <param name="factory">The target <see cref="TaskFactory"/>.</param>
        /// <param name="exception">The exception with which the <see cref="Task"/> should fault.</param>
        /// <returns>The completed <see cref="Task"/>.</returns>
        public static Task<TResult> FromException<TResult>(this TaskFactory factory, Exception exception)
        {
            var tcs = new TaskCompletionSource<TResult>(factory.CreationOptions);
            tcs.SetException(exception);
            return tcs.Task;
        }

        /// <summary>Creates a <see cref="Task"/> that has completed in the <see cref="TaskStatus.RanToCompletion"/> state with the specified result.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new <see cref="Task"/>.</typeparam>
        /// <param name="factory">The target <see cref="TaskFactory"/>.</param>
        /// <param name="result">The result with which the <see cref="Task"/> should complete.</param>
        /// <returns>The completed <see cref="Task"/>.</returns>
        public static Task<TResult> FromResult<TResult>(this TaskFactory factory, TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>(factory.CreationOptions);
            tcs.SetResult(result);
            return tcs.Task;
        }

        /// <summary>Creates a <see cref="Task"/> that has completed in the <see cref="TaskStatus.Canceled"/> state with the specified <see cref="CancellationToken"/>.</summary>
        /// <param name="factory">The target <see cref="TaskFactory"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> with which the <see cref="Task"/> should complete.</param>
        /// <returns>The completed <see cref="Task"/>.</returns>
        public static Task FromCancellation(this TaskFactory factory, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested) throw new ArgumentOutOfRangeException(nameof(cancellationToken));
            return new Task(() => { }, cancellationToken);
        }

        /// <summary>Creates a <see cref="Task"/> that has completed in the <see cref="TaskStatus.Canceled"/> state with the specified <see cref="CancellationToken"/>.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new <see cref="Task"/>.</typeparam>
        /// <param name="factory">The target <see cref="TaskFactory"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> with which the <see cref="Task"/> should complete.</param>
        /// <returns>The completed <see cref="Task"/>.</returns>
        public static Task<TResult> FromCancellation<TResult>(this TaskFactory factory, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested) throw new ArgumentOutOfRangeException(nameof(cancellationToken));
            return new Task<TResult>(DelegateCache<TResult>.DefaultResult, cancellationToken);
        }

        /// <summary>A cache of delegates.</summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        private class DelegateCache<TResult>
        {
            /// <summary>Function that returns default(TResult).</summary>
            internal static readonly Func<TResult> DefaultResult = () => default(TResult);
        }
        #endregion

        #region TaskFactory<TResult>
        /// <summary>Creates a <see cref="Task"/> that has completed in the <see cref="TaskStatus.Faulted"/> state with the specified exception.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new <see cref="Task"/>.</typeparam>
        /// <param name="factory">The target <see cref="TaskFactory"/>.</param>
        /// <param name="exception">The exception with which the <see cref="Task"/> should fault.</param>
        /// <returns>The completed <see cref="Task"/>.</returns>
        public static Task<TResult> FromException<TResult>(this TaskFactory<TResult> factory, Exception exception)
        {
            var tcs = new TaskCompletionSource<TResult>(factory.CreationOptions);
            tcs.SetException(exception);
            return tcs.Task;
        }

        /// <summary>Creates a <see cref="Task"/> that has completed in the <see cref="TaskStatus.RanToCompletion"/> state with the specified result.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new <see cref="Task"/>.</typeparam>
        /// <param name="factory">The target <see cref="TaskFactory"/>.</param>
        /// <param name="result">The result with which the <see cref="Task"/> should complete.</param>
        /// <returns>The completed <see cref="Task"/>.</returns>
        public static Task<TResult> FromResult<TResult>(this TaskFactory<TResult> factory, TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>(factory.CreationOptions);
            tcs.SetResult(result);
            return tcs.Task;
        }

        /// <summary>Creates a <see cref="Task"/> that has completed in the <see cref="TaskStatus.Canceled"/> state with the specified <see cref="CancellationToken"/>.</summary>
        /// <typeparam name="TResult">Specifies the type of payload for the new <see cref="Task"/>.</typeparam>
        /// <param name="factory">The target <see cref="TaskFactory"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> with which the <see cref="Task"/> should complete.</param>
        /// <returns>The completed <see cref="Task"/>.</returns>
        public static Task<TResult> FromCancellation<TResult>(this TaskFactory<TResult> factory, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested) throw new ArgumentOutOfRangeException(nameof(cancellationToken));
            return new Task<TResult>(DelegateCache<TResult>.DefaultResult, cancellationToken);
        }
        #endregion
    }
}