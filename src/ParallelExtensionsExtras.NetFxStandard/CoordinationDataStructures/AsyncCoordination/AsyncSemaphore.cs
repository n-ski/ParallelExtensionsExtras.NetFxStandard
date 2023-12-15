//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: AsyncSemaphore.cs
//
//--------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace System.Threading.Async;

/// <summary>Provides an asynchronous semaphore.</summary>
[DebuggerDisplay("CurrentCount={CurrentCount}, MaximumCount={MaximumCount}, WaitingCount={WaitingCount}")]
public sealed class AsyncSemaphore : IDisposable
{
    /// <summary>The current count.</summary>
    private int _currentCount;
    /// <summary>The maximum count. If _maxCount isn't positive, the instance has been disposed.</summary>
    private int _maxCount;
    /// <summary>Tasks waiting to be completed when the semaphore has count available.</summary>
    private Queue<TaskCompletionSource<object>> _waitingTasks;

    /// <summary>Initializes the <see cref="AsyncSemaphore"/> with a count of zero and a maximum count of <see cref="int.MaxValue"/>.</summary>
    public AsyncSemaphore() : this(0) { }

    /// <summary>Initializes the <see cref="AsyncSemaphore"/> with the specified count and a maximum count of <see cref="int.MaxValue"/>.</summary>
    /// <param name="initialCount">The initial count to use as the current count.</param>
    public AsyncSemaphore(int initialCount) : this(initialCount, Int32.MaxValue) { }

    /// <summary>Initializes the <see cref="AsyncSemaphore"/> with the specified counts.</summary>
    /// <param name="initialCount">The initial count to use as the current count.</param>
    /// <param name="maxCount">The maximum count allowed.</param>
    public AsyncSemaphore(int initialCount, int maxCount)
    {
        if (maxCount <= 0) throw new ArgumentOutOfRangeException(nameof(maxCount));
        if (initialCount > maxCount || initialCount < 0) throw new ArgumentOutOfRangeException(nameof(initialCount));
        _currentCount = initialCount;
        _maxCount = maxCount;
        _waitingTasks = new Queue<TaskCompletionSource<object>>();
    }

    /// <summary>Gets the current count.</summary>
    public int CurrentCount { get { return _currentCount; } }
    /// <summary>Gets the maximum count.</summary>
    public int MaximumCount { get { return _maxCount; } }
    /// <summary>Gets the number of operations currently waiting on the semaphore.</summary>
    public int WaitingCount { get { lock(_waitingTasks) return _waitingTasks.Count; } }

    /// <summary>Waits for a unit to be available in the semaphore.</summary>
    /// <returns>A <see cref="Task"/> that will be completed when a unit is available and this Wait operation succeeds.</returns>
    public Task WaitAsync()
    {
        ThrowIfDisposed();
        lock (_waitingTasks)
        {
            // If there's room, decrement the count and return a completed task
            if (_currentCount > 0)
            {
                _currentCount--;
#if NET40 || NET45
                    return CompletedTask.Default;
#else
                return Task.CompletedTask;
#endif
            }
            else
            {
                // Otherwise, cache a new task and return it
                var tcs = new TaskCompletionSource<object>();
                _waitingTasks.Enqueue(tcs);
                return tcs.Task;
            }
        }
    }

    /// <summary>
    /// Queues an action that will be executed when space is available
    /// in the semaphore.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the execution of the <paramref name="action"/>.
    /// </returns>
    /// <remarks>
    /// <see cref="Release"/> does not need to be called for this <paramref name="action"/>,
    /// as it will be handled implicitly by this method.
    /// </remarks>
    public Task Queue(Action action)
    {
        return WaitAsync().ContinueWith(_ =>
        {
            try { action(); }
            finally { Release(); }
        });
    }

    /// <summary>
    /// Queues a function that will be executed when space is available
    /// in the semaphore.
    /// </summary>
    /// <param name="function">The function to be executed.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the execution of the <paramref name="function"/>.
    /// </returns>
    /// <remarks>
    /// <see cref="Release"/> does not need to be called for this <paramref name="function"/>,
    /// as it will be handled implicitly by this method.
    /// </remarks>
    public Task<TResult> Queue<TResult>(Func<TResult> function)
    {
        return WaitAsync().ContinueWith(_ =>
        {
            try { return function(); }
            finally { Release(); }
        });
    }

    /// <summary>Releases a unit of work to the semaphore.</summary>
    public void Release()
    {
        ThrowIfDisposed();
        lock (_waitingTasks)
        {
            // Validate that there's room
            if (_currentCount == _maxCount) throw new SemaphoreFullException();

            // If there are any tasks waiting, allow one of them access
            if (_waitingTasks.Count > 0)
            {
                var tcs = _waitingTasks.Dequeue();
                tcs.SetResult(null);
            }
            // Otherwise, increment the available count
            else _currentCount++;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_maxCount <= 0) throw new ObjectDisposedException(GetType().Name);
    }

    /// <summary>Releases the resources used by the semaphore.</summary>
    public void Dispose()
    {
        if (_maxCount > 0)
        {
            _maxCount = 0;
            lock (_waitingTasks)
            {
                while (_waitingTasks.Count > 0)
                {
                    _waitingTasks.Dequeue().SetCanceled();
                }
            }
        }
    }
}