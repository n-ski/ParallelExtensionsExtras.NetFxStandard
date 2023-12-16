//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: TaskExtensions.cs
//
//--------------------------------------------------------------------------

using System.Linq;
#if NETFRAMEWORK || WINDOWS
using System.Windows.Threading;
#endif

namespace System.Threading.Tasks;

/// <summary>Extensions methods for <see cref="Task"/>.</summary>
public static class TaskExtrasExtensions
{
    #region ContinueWith accepting TaskFactory
    /// <summary>Creates a continuation task using the specified <see cref="TaskFactory"/>.</summary>
    /// <param name="task">The antecedent <see cref="Task"/>.</param>
    /// <param name="continuationAction">The continuation action.</param>
    /// <param name="factory">The <see cref="TaskFactory"/>.</param>
    /// <returns>A continuation task.</returns>
    public static Task ContinueWith(
        this Task task, Action<Task> continuationAction, TaskFactory factory)
    {
        return task.ContinueWith(continuationAction, factory.CancellationToken, factory.ContinuationOptions, factory.Scheduler ?? TaskScheduler.Current);
    }

    /// <summary>Creates a continuation task using the specified <see cref="TaskFactory"/>.</summary>
    /// <param name="task">The antecedent <see cref="Task"/>.</param>
    /// <param name="continuationFunction">The continuation function.</param>
    /// <param name="factory">The <see cref="TaskFactory"/>.</param>
    /// <returns>A continuation task.</returns>
    public static Task<TResult> ContinueWith<TResult>(
        this Task task, Func<Task, TResult> continuationFunction, TaskFactory factory)
    {
        return task.ContinueWith(continuationFunction, factory.CancellationToken, factory.ContinuationOptions, factory.Scheduler ?? TaskScheduler.Current);
    }
    #endregion

    #region ContinueWith accepting TaskFactory<TResult>
    /// <summary>Creates a continuation task using the specified <see cref="TaskFactory{TResult}"/>.</summary>
    /// <param name="task">The antecedent <see cref="Task{TResult}"/>.</param>
    /// <param name="continuationAction">The continuation action.</param>
    /// <param name="factory">The <see cref="TaskFactory{TResult}"/>.</param>
    /// <returns>A continuation task.</returns>
    public static Task ContinueWith<TResult>(
        this Task<TResult> task, Action<Task<TResult>> continuationAction, TaskFactory<TResult> factory)
    {
        return task.ContinueWith(continuationAction, factory.CancellationToken, factory.ContinuationOptions, factory.Scheduler ?? TaskScheduler.Current);
    }

    /// <summary>Creates a continuation task using the specified <see cref="TaskFactory{TResult}"/>.</summary>
    /// <param name="task">The antecedent <see cref="Task{TResult}"/>.</param>
    /// <param name="continuationFunction">The continuation function.</param>
    /// <param name="factory">The <see cref="TaskFactory{TResult}"/>.</param>
    /// <returns>A continuation task.</returns>
    public static Task<TNewResult> ContinueWith<TResult, TNewResult>(
        this Task<TResult> task, Func<Task<TResult>, TNewResult> continuationFunction, TaskFactory<TResult> factory)
    {
        return task.ContinueWith(continuationFunction, factory.CancellationToken, factory.ContinuationOptions, factory.Scheduler ?? TaskScheduler.Current);
    }
    #endregion

    #region ToAsync(AsyncCallback, object)
    /// <summary>
    /// Creates a <see cref="Task"/> that represents the completion of another <see cref="Task"/>, and 
    /// that schedules an <see cref="AsyncCallback"/> to run upon completion.
    /// </summary>
    /// <param name="task">The antecedent <see cref="Task"/>.</param>
    /// <param name="callback">The <see cref="AsyncCallback"/> to run.</param>
    /// <param name="state">The object state to use with the <see cref="AsyncCallback"/>.</param>
    /// <returns>The new <see cref="Task"/>.</returns>
    public static Task ToAsync(this Task task, AsyncCallback? callback, object? state)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        var tcs = new TaskCompletionSource<object>(state);
        task.ContinueWith(_ =>
        {
            tcs.SetFromTask(task);
            if (callback != null) callback(tcs.Task);
        });
        return tcs.Task;
    }

    /// <summary>
    /// Creates a <see cref="Task{TResult}"/> that represents the completion of another <see cref="Task{TResult}"/>, and
    /// that schedules an <see cref="AsyncCallback"/> to run upon completion.
    /// </summary>
    /// <param name="task">The antecedent <see cref="Task{TResult}"/>.</param>
    /// <param name="callback">The <see cref="AsyncCallback"/> to run.</param>
    /// <param name="state">The object state to use with the <see cref="AsyncCallback"/>.</param>
    /// <returns>The new <see cref="Task{TResult}"/>.</returns>
    public static Task<TResult> ToAsync<TResult>(this Task<TResult> task, AsyncCallback? callback, object? state)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        var tcs = new TaskCompletionSource<TResult>(state);
        task.ContinueWith(_ =>
        {
            tcs.SetFromTask(task);
            if (callback != null) callback(tcs.Task);
        });
        return tcs.Task;
    }
    #endregion

    #region Exception Handling
    /// <summary>Suppresses default exception handling of a <see cref="Task"/> that would otherwise reraise the exception on the finalizer thread.</summary>
    /// <param name="task">The <see cref="Task"/> to be monitored.</param>
    /// <returns>The original <see cref="Task"/>.</returns>
    public static Task IgnoreExceptions(this Task task)
    {
        task.ContinueWith(t => { var ignored = t.Exception; }, 
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted, 
            TaskScheduler.Default);
        return task;
    }

    /// <summary>Suppresses default exception handling of a <see cref="Task"/> that would otherwise reraise the exception on the finalizer thread.</summary>
    /// <param name="task">The <see cref="Task"/> to be monitored.</param>
    /// <returns>The original <see cref="Task"/>.</returns>
    public static Task<T> IgnoreExceptions<T>(this Task<T> task)
    {
        return (Task<T>)((Task)task).IgnoreExceptions();
    }

    /// <summary>Fails immediately when an exception is encountered.</summary>
    /// <param name="task">The <see cref="Task"/> to be monitored.</param>
    /// <returns>The original <see cref="Task"/>.</returns>
    public static Task FailFastOnException(this Task task)
    {
        task.ContinueWith(t => Environment.FailFast("A task faulted.", t.Exception),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
        return task;
    }

    /// <summary>Fails immediately when an exception is encountered.</summary>
    /// <param name="task">The <see cref="Task"/> to be monitored.</param>
    /// <returns>The original <see cref="Task"/>.</returns>
    public static Task<T> FailFastOnException<T>(this Task<T> task)
    {
        return (Task<T>)((Task)task).FailFastOnException();
    }

    /// <summary>Propagates any exceptions that occurred on the specified task.</summary>
    /// <param name="task">The <see cref="Task"/> whose exceptions are to be propagated.</param>
    public static void PropagateExceptions(this Task task)
    {
        if (!task.IsCompleted) throw new InvalidOperationException("The task has not completed.");
        if (task.IsFaulted) task.Wait();
    }

    /// <summary>Propagates any exceptions that occurred on the specified tasks.</summary>
    /// <param name="tasks">The <see cref="Task"/>s whose exceptions are to be propagated.</param>
    public static void PropagateExceptions(this Task [] tasks)
    {
        if (tasks == null) throw new ArgumentNullException(nameof(tasks));
        if (tasks.Any(t => t == null)) throw new NullReferenceException("A task is null.");
        if (tasks.Any(t => !t.IsCompleted)) throw new InvalidOperationException("A task has not completed.");
        Task.WaitAll(tasks);
    }
    #endregion

    #region Observables
    /// <summary>Creates an <see cref="IObservable{TResult}"/> that represents the completion of a <see cref="Task{TResult}"/>.</summary>
    /// <typeparam name="TResult">Specifies the type of data returned by the Task.</typeparam>
    /// <param name="task">The <see cref="Task{TResult}"/> to be represented as an IObservable.</param>
    /// <returns>An <see cref="IObservable{TResult}"/> that represents the completion of the <see cref="Task{TResult}"/>.</returns>
    public static IObservable<TResult> ToObservable<TResult>(this Task<TResult> task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        return new TaskObservable<TResult>(task);
    }

    /// <summary>An implementation of <see cref="IObservable{TResult}"/> that wraps a <see cref="Task{TResult}"/>.</summary>
    /// <typeparam name="TResult">The type of data returned by the task.</typeparam>
    private class TaskObservable<TResult> : IObservable<TResult>
    {
        internal readonly Task<TResult> _task;

        internal TaskObservable(Task<TResult> task)
        {
            _task = task;
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            // Validate arguments
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            // Support cancelling the continuation if the observer is unsubscribed
            var cts = new CancellationTokenSource();

            // Create a continuation to pass data along to the observer
            _task.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        observer.OnNext(_task.Result);
                        observer.OnCompleted();
                        break;

                    case TaskStatus.Faulted:
                        observer.OnError(_task.Exception!);
                        break;

                    case TaskStatus.Canceled:
                        observer.OnError(new TaskCanceledException(t));
                        break;
                }
            }, cts.Token);

            // Support unsubscribe simply by canceling the continuation if it hasn't yet run
            return new CancelOnDispose(cts);
        }
    }

    /// <summary>Translate a call to IDisposable.Dispose to a CancellationTokenSource.Cancel.</summary>
    private class CancelOnDispose : IDisposable
    {
        internal readonly CancellationTokenSource Source;

        internal CancelOnDispose(CancellationTokenSource source)
        {
            Source = source;
        }

        void IDisposable.Dispose() { Source.Cancel(); }
    }
    #endregion

    #region Timeouts
    /// <summary>Creates a new <see cref="Task"/> that mirrors the supplied task but that will be canceled after the specified timeout.</summary>
    /// <param name="task">The task.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>The new <see cref="Task"/> that may time out.</returns>
    public static Task WithTimeout(this Task task, TimeSpan timeout)
    {
        var result = new TaskCompletionSource<object>(task.AsyncState);
        var timer = new Timer(state => ((TaskCompletionSource<object>?)state!).TrySetCanceled(), result, timeout, TimeSpan.FromMilliseconds(-1));
        task.ContinueWith(t =>
        {
            timer.Dispose();
            result.TrySetFromTask(t);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return result.Task;
    }

    /// <summary>Creates a new <see cref="Task{TResult}"/> that mirrors the supplied task but that will be canceled after the specified timeout.</summary>
    /// <typeparam name="TResult">Specifies the type of data contained in the task.</typeparam>
    /// <param name="task">The task.</param>
    /// <param name="timeout">The timeout.</param>
    /// <returns>The new <see cref="Task{TResult}"/> that may time out.</returns>
    public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        var result = new TaskCompletionSource<TResult>(task.AsyncState);
        var timer = new Timer(state => ((TaskCompletionSource<TResult>?)state!).TrySetCanceled(), result, timeout, TimeSpan.FromMilliseconds(-1));
        task.ContinueWith(t =>
        {
            timer.Dispose();
            result.TrySetFromTask(t);
        }, TaskContinuationOptions.ExecuteSynchronously);
        return result.Task;
    }
    #endregion

    #region Children
    /// <summary>
    /// Ensures that a parent task can't transition into a completed state
    /// until the specified task has also completed, even if it's not
    /// already a child task.
    /// </summary>
    /// <param name="task">The task to attach to the current task as a child.</param>
    public static void AttachToParent(this Task task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        task.ContinueWith(t => t.Wait(), CancellationToken.None,
            TaskContinuationOptions.AttachedToParent |
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }
    #endregion

    #region Waiting
#if NETFRAMEWORK || WINDOWS
    /// <summary>Waits for the <see cref="Task"/> to complete execution, pumping in the meantime.</summary>
    /// <param name="task">The <see cref="Task"/> for which to wait.</param>
    /// <remarks>This method is intended for usage with Windows Presentation Foundation.</remarks>
    public static void WaitWithPumping(this Task task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        var nestedFrame = new DispatcherFrame();
        task.ContinueWith(_ => nestedFrame.Continue = false);
        Dispatcher.PushFrame(nestedFrame);
        task.Wait();
    }
#endif

    /// <summary>Waits for the <see cref="Task"/> to complete execution, returning the <see cref="Task"/>'s final status.</summary>
    /// <param name="task">The <see cref="Task"/> for which to wait.</param>
    /// <returns>The completion status of the <see cref="Task"/>.</returns>
    /// <remarks>Unlike <see cref="Task.Wait()"/>, this method will not throw an exception if the task ends in the <see cref="TaskStatus.Faulted"/> or <see cref="TaskStatus.Canceled"/> state.</remarks>
    public static TaskStatus WaitForCompletionStatus(this Task task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        ((IAsyncResult)task).AsyncWaitHandle.WaitOne();
        return task.Status;
    }
    #endregion

    #region Then
    /// <summary>Creates a <see cref="Task"/> that represents the completion of a follow-up <paramref name="action"/> when a <paramref name="task"/> completes.</summary>
    /// <param name="task">The task.</param>
    /// <param name="action">The action to run when the <see cref="Task"/> completes.</param>
    /// <returns>The <see cref="Task"/> that represents the completion of both the <paramref name="task"/> and the <paramref name="action"/>.</returns>
    public static Task Then(this Task task, Action action)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (action == null) throw new ArgumentNullException(nameof(action));

        var tcs = new TaskCompletionSource<object?>();
        task.ContinueWith(delegate
        {
            if (task.IsFaulted) tcs.TrySetException(task.Exception!.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                try
                {
                    action();
                    tcs.TrySetResult(null);
                }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }

    /// <summary>Creates a <see cref="Task{TResult}"/> that represents the completion of a follow-up <paramref name="function"/> when a <paramref name="task"/> completes.</summary>
    /// <param name="task">The task.</param>
    /// <param name="function">The function to run when the <see cref="Task{TResult}"/> completes.</param>
    /// <returns>The <see cref="Task{TResult}"/> that represents the completion of both the <paramref name="task"/> and the <paramref name="function"/>.</returns>
    public static Task<TResult> Then<TResult>(this Task task, Func<TResult> function)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (function == null) throw new ArgumentNullException(nameof(function));

        var tcs = new TaskCompletionSource<TResult>();
        task.ContinueWith(delegate
        {
            if (task.IsFaulted) tcs.TrySetException(task.Exception!.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                try
                {
                    var result = function();
                    tcs.TrySetResult(result);
                }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }

    /// <summary>Creates a <see cref="Task{TResult}"/> that represents the completion of a follow-up <paramref name="action"/> when a <paramref name="task"/> completes.</summary>
    /// <param name="task">The task.</param>
    /// <param name="action">The action to run when the <see cref="Task{TResult}"/> completes.</param>
    /// <returns>The <see cref="Task{TResult}"/> that represents the completion of both the <paramref name="task"/> and the <paramref name="action"/>.</returns>
    public static Task Then<TResult>(this Task<TResult> task, Action<TResult> action)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (action == null) throw new ArgumentNullException(nameof(action));

        var tcs = new TaskCompletionSource<object?>();
        task.ContinueWith(delegate
        {
            if (task.IsFaulted) tcs.TrySetException(task.Exception!.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                try
                {
                    action(task.Result);
                    tcs.TrySetResult(null);
                }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }

    /// <summary>Creates a <see cref="Task"/> that represents the completion of a follow-up <paramref name="function"/> when a <paramref name="task"/> completes.</summary>
    /// <param name="task">The task.</param>
    /// <param name="function">The function to run when the task completes.</param>
    /// <returns>The <see cref="Task"/> that represents the completion of both the <paramref name="task"/> and the <paramref name="function"/>.</returns>
    public static Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task, Func<TResult, TNewResult> function)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (function == null) throw new ArgumentNullException(nameof(function));

        var tcs = new TaskCompletionSource<TNewResult>();
        task.ContinueWith(delegate
        {
            if (task.IsFaulted) tcs.TrySetException(task.Exception!.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                try
                {
                    var result = function(task.Result);
                    tcs.TrySetResult(result);
                }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }

    /// <summary>Creates a <see cref="Task"/> that represents the completion of a second task when a first task completes.</summary>
    /// <param name="task">The first task.</param>
    /// <param name="next">The function that produces the second task.</param>
    /// <returns>The <see cref="Task"/> that represents the completion of both the first and second task.</returns>
    public static Task Then(this Task task, Func<Task> next)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (next == null) throw new ArgumentNullException(nameof(next));

        var tcs = new TaskCompletionSource<object>();
        task.ContinueWith(delegate
        {
            // When the first task completes, if it faulted or was canceled, bail
            if (task.IsFaulted) tcs.TrySetException(task.Exception!.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                // Otherwise, get the next task.  If it's null, bail.  If not,
                // when it's done we'll have our result.
                try { next().ContinueWith(t => tcs.TrySetFromTask(t), TaskScheduler.Default); }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }

    /// <summary>Creates a <see cref="Task"/> that represents the completion of a second task when a first task completes.</summary>
    /// <param name="task">The first task.</param>
    /// <param name="next">The function that produces the second task based on the result of the first task.</param>
    /// <returns>The <see cref="Task"/> that represents the completion of both the first and second task.</returns>
    public static Task Then<T>(this Task<T> task, Func<T, Task> next)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (next == null) throw new ArgumentNullException(nameof(next));

        var tcs = new TaskCompletionSource<object>();
        task.ContinueWith(delegate
        {
            // When the first task completes, if it faulted or was canceled, bail
            if (task.IsFaulted) tcs.TrySetException(task.Exception!.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                // Otherwise, get the next task.  If it's null, bail.  If not,
                // when it's done we'll have our result.
                try { next(task.Result).ContinueWith(t => tcs.TrySetFromTask(t), TaskScheduler.Default); }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }

    /// <summary>Creates a <see cref="Task"/> that represents the completion of a second task when a first task completes.</summary>
    /// <param name="task">The first task.</param>
    /// <param name="next">The function that produces the second task.</param>
    /// <returns>The <see cref="Task"/> that represents the completion of both the first and second task.</returns>
    public static Task<TResult> Then<TResult>(this Task task, Func<Task<TResult>> next)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (next == null) throw new ArgumentNullException(nameof(next));

        var tcs = new TaskCompletionSource<TResult>();
        task.ContinueWith(delegate
        {
            // When the first task completes, if it faulted or was canceled, bail
            if (task.IsFaulted) tcs.TrySetException(task.Exception!.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                // Otherwise, get the next task.  If it's null, bail.  If not,
                // when it's done we'll have our result.
                try { next().ContinueWith(t => tcs.TrySetFromTask(t), TaskScheduler.Default); }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }

    /// <summary>Creates a <see cref="Task"/> that represents the completion of a second task when a first task completes.</summary>
    /// <param name="task">The first task.</param>
    /// <param name="next">The function that produces the second task based on the result of the first.</param>
    /// <returns>The <see cref="Task"/> that represents the completion of both the first and second task.</returns>
    public static Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task, Func<TResult, Task<TNewResult>> next)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (next == null) throw new ArgumentNullException(nameof(next));

        var tcs = new TaskCompletionSource<TNewResult>();
        task.ContinueWith(delegate
        {
            // When the first task completes, if it faulted or was canceled, bail
            if (task.IsFaulted) tcs.TrySetException(task.Exception!.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                // Otherwise, get the next task.  If it's null, bail.  If not,
                // when it's done we'll have our result.
                try { next(task.Result).ContinueWith(t => tcs.TrySetFromTask(t), TaskScheduler.Default); }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }
    #endregion
}
