//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: TaskFactoryExtensions_Common.cs
//
//--------------------------------------------------------------------------

namespace System.Threading.Tasks;

/// <summary>Extensions for <see cref="TaskFactory"/>.</summary>
public static partial class TaskFactoryExtensions
{
    /// <summary>Creates a generic <see cref="TaskFactory"/> from a non-generic one.</summary>
    /// <typeparam name="TResult">Specifies the type of <see cref="Task"/> results for the <see cref="Task"/>s created by the new <see cref="TaskFactory"/>.</typeparam>
    /// <param name="factory">The <see cref="TaskFactory"/> to serve as a template.</param>
    /// <returns>The created <see cref="TaskFactory"/>.</returns>
    public static TaskFactory<TResult> ToGeneric<TResult>(this TaskFactory factory)
    {
        return new TaskFactory<TResult>(
            factory.CancellationToken, factory.CreationOptions, factory.ContinuationOptions, factory.Scheduler);
    }

    /// <summary>Creates a generic <see cref="TaskFactory"/> from a non-generic one.</summary>
    /// <typeparam name="TResult">Specifies the type of <see cref="Task"/> results for the <see cref="Task"/>s created by the new <see cref="TaskFactory"/>.</typeparam>
    /// <param name="factory">The <see cref="TaskFactory"/> to serve as a template.</param>
    /// <returns>The created <see cref="TaskFactory"/>.</returns>
    public static TaskFactory ToNonGeneric<TResult>(this TaskFactory<TResult> factory)
    {
        return new TaskFactory(
            factory.CancellationToken, factory.CreationOptions, factory.ContinuationOptions, factory.Scheduler);
    }

    /// <summary>Gets the <see cref="TaskScheduler"/> instance that should be used to schedule tasks.</summary>
    public static TaskScheduler GetTargetScheduler(this TaskFactory factory)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        return factory.Scheduler ?? TaskScheduler.Current;
    }

    /// <summary>Gets the <see cref="TaskScheduler"/> instance that should be used to schedule tasks.</summary>
    public static TaskScheduler GetTargetScheduler<TResult>(this TaskFactory<TResult> factory)
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        return factory.Scheduler != null ? factory.Scheduler : TaskScheduler.Current;
    }

    /// <summary>Converts <see cref="TaskCreationOptions"/> into <see cref="TaskContinuationOptions"/>.</summary>
    /// <param name="creationOptions"></param>
    /// <returns></returns>
    private static TaskContinuationOptions ContinuationOptionsFromCreationOptions(TaskCreationOptions creationOptions)
    {
        return (TaskContinuationOptions)
            ((creationOptions & TaskCreationOptions.AttachedToParent) |
                (creationOptions & TaskCreationOptions.PreferFairness) |
                (creationOptions & TaskCreationOptions.LongRunning));
    }
}