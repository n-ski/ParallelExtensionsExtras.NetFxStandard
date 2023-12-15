//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: SpinLockClass.cs
//
//--------------------------------------------------------------------------

namespace System.Threading;

/// <summary>Provides a simple, reference type wrapper for <see cref="SpinLock"/>.</summary>
public class SpinLockClass
{
    private SpinLock _spinLock; // NOTE: must *not* be readonly due to SpinLock being a mutable struct

    /// <summary>Initializes an instance of the <see cref="SpinLockClass"/>.</summary>
    public SpinLockClass()
    {
        _spinLock = new SpinLock();
    }

    /// <summary>Initializes an instance of the <see cref="SpinLockClass"/>.</summary>
    /// <param name="enableThreadOwnerTracking">
    /// Controls whether the <see cref="SpinLockClass"/> should track
    /// thread-ownership of the lock.
    /// </param>
    public SpinLockClass(bool enableThreadOwnerTracking)
    {
        _spinLock = new SpinLock(enableThreadOwnerTracking);
    }

    /// <summary>Runs the specified delegate under the lock.</summary>
    /// <param name="runUnderLock">The delegate to be executed while holding the lock.</param>
    public void Execute(Action runUnderLock)
    {
        bool lockTaken = false;
        try
        {
            Enter(ref lockTaken);
            runUnderLock();
        }
        finally
        {
            if (lockTaken) Exit();
        }
    }

    /// <summary>Enters the lock.</summary>
    /// <param name="lockTaken">
    /// Upon exit of the <see cref="Enter(ref bool)"/> method, specifies whether the lock was acquired. 
    /// The variable passed by reference must be initialized to <see langword="false"/>.
    /// </param>
    public void Enter(ref bool lockTaken)
    {
        _spinLock.Enter(ref lockTaken);
    }

    /// <summary>Exits the <see cref="SpinLock"/>.</summary>
    public void Exit()
    {
        _spinLock.Exit();
    }

    /// <summary>Exits the <see cref="SpinLock"/>.</summary>
    /// <param name="useMemoryBarrier">
    /// A <see cref="bool"/> value that indicates whether a memory fence should be issued in
    /// order to immediately publish the exit operation to other threads.
    /// </param>
    public void Exit(bool useMemoryBarrier)
    {
        _spinLock.Exit(useMemoryBarrier);
    }
}