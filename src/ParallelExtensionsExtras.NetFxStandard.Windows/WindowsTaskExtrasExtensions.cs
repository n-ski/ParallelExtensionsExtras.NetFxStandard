//--------------------------------------------------------------------------
//
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//
//  File: WindowsTaskExtrasExtensions.cs
//
//--------------------------------------------------------------------------

using System.Windows.Threading;

namespace System.Threading.Tasks;

/// <summary>Extensions methods for <see cref="Task"/>.</summary>
public static class WindowsTaskExtrasExtensions
{
    #region Waiting

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

    #endregion
}
