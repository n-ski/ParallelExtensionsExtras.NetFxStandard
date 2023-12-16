//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: CompletedTask.cs
//
//--------------------------------------------------------------------------

namespace System.Threading.Tasks;
#if NET40 || NET45
/// <summary>Provides access to an already completed task.</summary>
/// <remarks>A completed task can be useful for using <see cref="Task"/>.ContinueWith overloads where there aren't <see cref="TaskFactory"/>.StartNew equivalents.</remarks>
public static class CompletedTask
{
    /// <summary>Gets a completed <see cref="Task"/>.</summary>
    public readonly static Task Default = CompletedTask<object>.Default;
}

/// <summary>Provides access to an already completed task.</summary>
/// <remarks>A completed task can be useful for using <see cref="Task"/>.ContinueWith overloads where there aren't <see cref="TaskFactory"/>.StartNew equivalents.</remarks>
public static class CompletedTask<TResult>
{
    /// <summary>Initializes a <see cref="Task{TResult}"/>.</summary>
    static CompletedTask()
    {
        var tcs = new TaskCompletionSource<TResult?>();
        tcs.TrySetResult(default(TResult));
        Default = tcs.Task;
    }

    /// <summary>Gets a completed <see cref="Task{TResult}"/>.</summary>
    public readonly static Task<TResult?> Default;
}
#endif
