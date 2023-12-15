//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: CurrentThreadTaskScheduler.cs
//
//--------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>Provides a task scheduler that runs tasks on the current thread.</summary>
    public sealed class CurrentThreadTaskScheduler : TaskScheduler
    {
        /// <summary>Runs the provided <see cref="Task"/> synchronously on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        protected override void QueueTask(Task task)
        {
            TryExecuteTask(task);
        }

        /// <summary>Runs the provided Task synchronously on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Whether the <paramref name="task"/> was previously queued to the scheduler.</param>
        /// <returns><see langword="true"/> if the <paramref name="task"/> was successfully executed; otherwise, <see langword="false"/>.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return TryExecuteTask(task);
        }

        /// <summary>Gets the <see cref="Task"/>s currently scheduled to this scheduler.</summary>
        /// <returns>An empty enumerable, as <see cref="Task"/>s are never queued, only executed.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return Enumerable.Empty<Task>();
        }

        /// <summary>Gets the maximum degree of parallelism for this scheduler.</summary>
        public override int MaximumConcurrencyLevel { get { return 1; } }
    }
}