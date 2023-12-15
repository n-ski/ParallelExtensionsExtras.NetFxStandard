//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: CancellationTokenExtensions.cs
//
//--------------------------------------------------------------------------

using System.Collections.Concurrent.Partitioners;
using System.Collections.Generic;

namespace System.Threading;

/// <summary>Extension methods for <see cref="CancellationToken"/>.</summary>
public static class CancellationTokenExtensions
{
    /// <summary>Cancels a <see cref="CancellationTokenSource"/> and throws a corresponding <see cref="OperationCanceledException"/>.</summary>
    /// <param name="source">The source to be canceled.</param>
    public static void CancelAndThrow(this CancellationTokenSource source)
    {
        source.Cancel();
        source.Token.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Creates a <see cref="CancellationTokenSource"/> that will be canceled when the specified token has cancellation requested.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The created <see cref="CancellationTokenSource"/>.</returns>
    public static CancellationTokenSource CreateLinkedSource(this CancellationToken token)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(token, new CancellationToken());
    }
}