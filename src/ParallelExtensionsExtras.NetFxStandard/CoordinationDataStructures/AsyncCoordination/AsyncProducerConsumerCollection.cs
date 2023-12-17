﻿//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: AsyncProducerConsumerCollection.cs
//
//--------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace System.Threading.Async;

/// <summary>Provides an asynchronous producer/consumer collection.</summary>
[DebuggerDisplay("Count={CurrentCount}")]
public sealed class AsyncProducerConsumerCollection<T> : IDisposable
{
    /// <summary>Asynchronous semaphore used to keep track of asynchronous work.</summary>
    private AsyncSemaphore? _semaphore = new AsyncSemaphore();
    /// <summary>The data stored in the collection.</summary>
    private IProducerConsumerCollection<T> _collection;

    /// <summary>Initializes the asynchronous producer/consumer collection to store data in a first-in-first-out (FIFO) order.</summary>
    public AsyncProducerConsumerCollection() : this(new ConcurrentQueue<T>()) { }

    /// <summary>Initializes the asynchronous producer/consumer collection.</summary>
    /// <param name="collection">The underlying collection to use to store data.</param>
    public AsyncProducerConsumerCollection(IProducerConsumerCollection<T> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        _collection = collection;
    }

    /// <summary>Adds an element to the collection.</summary>
    /// <param name="item">The item to be added.</param>
    public void Add(T item)
    {
        ThrowIfDisposed();

        if (_collection.TryAdd(item)) _semaphore.Release();
        else throw new InvalidOperationException("Invalid collection");
    }

    /// <summary>Takes an element from the collection asynchronously.</summary>
    /// <returns>A <see cref="Task"/> that represents the element removed from the collection.</returns>
    public Task<T> Take()
    {
        ThrowIfDisposed();

        return _semaphore.WaitAsync().ContinueWith(_ =>
        {
            if (!_collection.TryTake(out var result)) throw new InvalidOperationException("Invalid collection");
            return result;
        }, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    /// <summary>Gets the number of elements in the collection.</summary>
    public int Count { get { return _collection.Count; } }

    /// <summary>Disposes of the collection.</summary>
    public void Dispose()
    {
        if (_semaphore != null)
        {
            _semaphore.Dispose();
            _semaphore = null;
        }
    }

    [MemberNotNull(nameof(_semaphore))]
    private void ThrowIfDisposed()
    {
        if (_semaphore == null) throw new ObjectDisposedException(GetType().FullName);
    }
}
