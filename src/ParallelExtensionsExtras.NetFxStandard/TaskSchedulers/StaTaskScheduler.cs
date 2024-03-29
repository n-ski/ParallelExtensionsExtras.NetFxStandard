﻿//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: StaTaskScheduler.cs
//
//--------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Threading.Tasks.Schedulers;

/// <summary>Provides a scheduler that uses STA threads.</summary>
#if NET
[Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
public sealed class StaTaskScheduler : TaskScheduler, IDisposable
{
    /// <summary>Stores the queued tasks to be executed by our pool of STA threads.</summary>
    private BlockingCollection<Task>? _tasks;
    /// <summary>The STA threads used by the scheduler.</summary>
    private readonly List<Thread> _threads;

    /// <summary>Initializes a new instance of the <see cref="StaTaskScheduler"/> class with the specified concurrency level.</summary>
    /// <param name="numberOfThreads">The number of threads that should be created and used by this scheduler.</param>
    public StaTaskScheduler(int numberOfThreads)
    {
        // Validate arguments
        if (numberOfThreads < 1) throw new ArgumentOutOfRangeException(nameof(numberOfThreads));

        // Initialize the tasks collection
        _tasks = new BlockingCollection<Task>();

        // Create the threads to be used by this scheduler
        _threads = Enumerable.Range(0, numberOfThreads).Select(i =>
        {
            var thread = new Thread(() =>
            {
                // Continually get the next task and try to execute it.
                // This will continue until the scheduler is disposed and no more tasks remain.
                foreach (var t in _tasks.GetConsumingEnumerable())
                {
                    TryExecuteTask(t);
                }
            });
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            return thread;
        }).ToList();

        // Start all of the threads
        _threads.ForEach(t => t.Start());
    }

    /// <summary>Queues a <see cref="Task"/> to be executed by this scheduler.</summary>
    /// <param name="task">The task to be executed.</param>
    /// <exception cref="ObjectDisposedException">Object was disposed.</exception>
    protected override void QueueTask(Task task)
    {
        ThrowIfDisposed();

        // Push it into the blocking collection of tasks
        _tasks.Add(task);
    }

    /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
    /// <returns>An enumerable of all tasks currently scheduled.</returns>
    /// <exception cref="ObjectDisposedException">Object was disposed.</exception>
    protected override IEnumerable<Task> GetScheduledTasks()
    {
        ThrowIfDisposed();

        // Serialize the contents of the blocking collection of tasks for the debugger
        return _tasks.ToArray();
    }

    /// <summary>Determines whether a <see cref="Task"/> may be inlined.</summary>
    /// <param name="task">The task to be executed.</param>
    /// <param name="taskWasPreviouslyQueued">Whether the <paramref name="task"/> was previously queued.</param>
    /// <returns><see langword="true"/> if the <paramref name="task"/> was successfully inlined; otherwise, <see langword="false"/>.</returns>
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        // Try to inline if the current thread is STA
        return
            Thread.CurrentThread.GetApartmentState() == ApartmentState.STA &&
            TryExecuteTask(task);
    }

    /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
    public override int MaximumConcurrencyLevel
    {
        get { return _threads.Count; }
    }

    /// <summary>
    /// Cleans up the scheduler by indicating that no more tasks will be queued.
    /// This method blocks until all threads successfully shutdown.
    /// </summary>
    public void Dispose()
    {
        if (_tasks != null)
        {
            // Indicate that no new tasks will be coming in
            _tasks.CompleteAdding();

            // Wait for all threads to finish processing tasks
            foreach (var thread in _threads) thread.Join();

            // Cleanup
            _tasks.Dispose();
            _tasks = null;
        }
    }

    [MemberNotNull(nameof(_tasks))]
    private void ThrowIfDisposed()
    {
        if (_tasks == null) throw new ObjectDisposedException(GetType().FullName);
    }
}
