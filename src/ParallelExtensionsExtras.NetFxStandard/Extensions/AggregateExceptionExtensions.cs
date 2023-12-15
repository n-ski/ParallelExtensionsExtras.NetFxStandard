//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: AggregateExceptionExtensions.cs
//
//--------------------------------------------------------------------------

using System.Collections.Generic;

namespace System;

/// <summary>Extension methods for <see cref="AggregateException"/>.</summary>
public static class AggregateExceptionExtensions
{
    /// <summary>Invokes a handler on each <see cref="Exception"/> contained by this <see cref="AggregateException"/>.</summary>
    /// <param name="aggregateException">The <see cref="AggregateException"/>.</param>
    /// <param name="predicate">
    /// The predicate to execute for each exception. The predicate accepts as an argument the <see cref="Exception"/>
    /// to be processed and returns a <see cref="bool"/> to indicate whether the exception was handled.
    /// </param>
    /// <param name="leaveStructureIntact">
    /// Whether the rethrown <see cref="AggregateException"/> should maintain the same hierarchy as the original.
    /// </param>
    public static void Handle(
        this AggregateException aggregateException,
        Func<Exception, bool> predicate, bool leaveStructureIntact)
    {
        if (aggregateException == null) throw new ArgumentNullException(nameof(aggregateException));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        // If leaveStructureIntact, use this implementation
        if (leaveStructureIntact)
        {
            var result = HandleRecursively(aggregateException, predicate);
            if (result != null) throw result;
        }
        // Otherwise, default back to the implementation on AggregateException
        else aggregateException.Handle(predicate);
    }

    private static AggregateException HandleRecursively(
        AggregateException aggregateException, Func<Exception, bool> predicate)
    {   
        // Maintain a list of exceptions to be rethrown
        List<Exception> innerExceptions = null;

        // Loop over all of the inner exceptions
        foreach(var inner in aggregateException.InnerExceptions)
        {
            // If the inner exception is itself an aggregate, process recursively
            AggregateException innerAsAggregate = inner as AggregateException;
            if (innerAsAggregate != null)
            {
                // Process recursively, and if we get back a new aggregate, store it
                AggregateException newChildAggregate = HandleRecursively(innerAsAggregate, predicate);
                if (newChildAggregate != null)
                {
                    if (innerExceptions != null) innerExceptions = new List<Exception>();
                    innerExceptions.Add(newChildAggregate);
                }
            }
            // Otherwise, if the exception does not match the filter, store it
            else if (!predicate(inner))
            {
                if (innerExceptions != null) innerExceptions = new List<Exception>();
                innerExceptions.Add(inner);
            }
        }
            
        // If there are any remaining exceptions, return them in a new aggregate.
        return innerExceptions.Count > 0 ?
            new AggregateException(aggregateException.Message, innerExceptions) :
            null;
    }
}