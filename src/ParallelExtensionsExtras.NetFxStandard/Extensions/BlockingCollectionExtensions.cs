﻿//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: BlockingCollectionExtensions.cs
//
//--------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Linq;

namespace System.Collections.Concurrent;

/// <summary>Extension methods for <see cref="BlockingCollection{T}"/>.</summary>
public static class BlockingCollectionExtensions
{
    /// <summary>
    /// Gets a partitioner for a <see cref="BlockingCollection{T}"/> that consumes and yields the contents of the <see cref="BlockingCollection{T}"/>.</summary>
    /// <typeparam name="T">Specifies the type of data in the collection.</typeparam>
    /// <param name="collection">The collection for which to create a partitioner.</param>
    /// <returns>A partitioner that completely consumes and enumerates the contents of the collection.</returns>
    /// <remarks>
    /// Using this partitioner with a <see cref="Threading.Tasks.Parallel"/>.ForEach loop or with PLINQ eliminates the need for those
    /// constructs to do any additional locking.  The only synchronization in place is that used by the
    /// <see cref="BlockingCollection{T}"/> internally.
    /// </remarks>
    public static Partitioner<T> GetConsumingPartitioner<T>(this BlockingCollection<T> collection)
    {
        return new BlockingCollectionPartitioner<T>(collection);
    }

    /// <summary>Provides a partitioner that consumes a blocking collection and yields its contents.</summary>
    /// <typeparam name="T">Specifies the type of data in the collection.</typeparam>
    private class BlockingCollectionPartitioner<T> : Partitioner<T>
    {
        /// <summary>The target collection.</summary>
        private BlockingCollection<T> _collection;

        /// <summary>Initializes the partitioner.</summary>
        /// <param name="collection">The collection to be enumerated and consumed.</param>
        internal BlockingCollectionPartitioner(BlockingCollection<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            _collection = collection;
        }

        /// <summary>Gets whether additional partitions can be created dynamically.</summary>
        public override bool SupportsDynamicPartitions { get { return true; } }

        /// <summary>Partitions the underlying collection into the given number of partitions.</summary>
        /// <param name="partitionCount">The number of partitions to create.</param>
        /// <returns>A list containing partitionCount enumerators.</returns>
        public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
        {
            if (partitionCount < 1) throw new ArgumentOutOfRangeException(nameof(partitionCount));
            var dynamicPartitioner = GetDynamicPartitions();
            return Enumerable.Range(0, partitionCount).Select(_ => dynamicPartitioner.GetEnumerator()).ToArray();
        }

        /// <summary>
        /// Creates an object that can partition the underlying collection into a variable number of partitions.
        /// </summary>
        /// <returns>An object that can create partitions over the underlying data source.</returns>
        public override IEnumerable<T> GetDynamicPartitions()
        {
            return _collection.GetConsumingEnumerable();
        }
    }

    /// <summary>Adds the contents of an <see cref="IEnumerable{T}"/> to the <see cref="BlockingCollection{T}"/>.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="target">The target <see cref="BlockingCollection{T}"/> to be augmented.</param>
    /// <param name="source">The source enumerable containing the data to be added.</param>
    /// <param name="completeAddingWhenDone">
    /// Whether to mark the <paramref name="target"/> <see cref="BlockingCollection{T}"/> as complete for adding when 
    /// all elements of the <paramref name="source"/> <see cref="IEnumerable{T}"/> have been transfered.
    /// </param>
    public static void AddFromEnumerable<T>(this BlockingCollection<T> target, IEnumerable<T> source, bool completeAddingWhenDone)
    {
        try { foreach (var item in source) target.Add(item); }
        finally { if (completeAddingWhenDone) target.CompleteAdding(); }
    }

    /// <summary>Adds the contents of an <see cref="IObservable{T}"/> to the <see cref="BlockingCollection{T}"/>.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="target">The target <see cref="BlockingCollection{T}"/> to be augmented.</param>
    /// <param name="source">The source observable containing the data to be added.</param>
    /// <param name="completeAddingWhenDone">
    /// Whether to mark the <paramref name="target"/> <see cref="BlockingCollection{T}"/> as complete for adding when 
    /// all elements of the <paramref name="source"/> <see cref="IObservable{T}"/> have been transfered.
    /// </param>
    /// <returns>An <see cref="IDisposable"/> that may be used to cancel the transfer.</returns>
    public static IDisposable AddFromObservable<T>(this BlockingCollection<T> target, IObservable<T> source, bool completeAddingWhenDone)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source.Subscribe(new DelegateBasedObserver<T>
        (
            onNext: item => target.Add(item),
            onError: error => { if (completeAddingWhenDone) target.CompleteAdding(); },
            onCompleted: () => { if (completeAddingWhenDone) target.CompleteAdding(); }
        ));
    }

    /// <summary>Creates an <see cref="IProducerConsumerCollection{T}"/>-facade for a <see cref="BlockingCollection{T}"/> instance.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="collection">The <see cref="BlockingCollection{T}"/>.</param>
    /// <returns>
    /// An <see cref="IProducerConsumerCollection{T}"/> that wraps the provided <see cref="BlockingCollection{T}"/>.
    /// </returns>
    public static IProducerConsumerCollection<T> ToProducerConsumerCollection<T>(
        this BlockingCollection<T> collection)
    {
        return ToProducerConsumerCollection(collection, Timeout.Infinite);
    }

    /// <summary>Creates an <see cref="IProducerConsumerCollection{T}"/>-facade for a <see cref="BlockingCollection{T}"/> instance.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="collection">The <see cref="BlockingCollection{T}"/>.</param>
    /// <param name="millisecondsTimeout">-1 for infinite blocking add and take operations. 0 for non-blocking, 1 or greater for blocking with timeout.</param>
    /// An <see cref="IProducerConsumerCollection{T}"/> that wraps the provided <see cref="BlockingCollection{T}"/>.
    public static IProducerConsumerCollection<T> ToProducerConsumerCollection<T>(
        this BlockingCollection<T> collection, int millisecondsTimeout)
    {
        return new ProducerConsumerWrapper<T>(collection, millisecondsTimeout, new CancellationToken());
    }

    /// <summary>Creates an <see cref="IProducerConsumerCollection{T}"/>-facade for a <see cref="BlockingCollection{T}"/> instance.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="collection">The <see cref="BlockingCollection{T}"/>.</param>
    /// <param name="millisecondsTimeout">-1 for infinite blocking add and take operations. 0 for non-blocking, 1 or greater for blocking with timeout.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for any blocking operations.</param>
    /// An <see cref="IProducerConsumerCollection{T}"/> that wraps the provided <see cref="BlockingCollection{T}"/>.
    public static IProducerConsumerCollection<T> ToProducerConsumerCollection<T>(
        this BlockingCollection<T> collection, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        return new ProducerConsumerWrapper<T>(collection, millisecondsTimeout, cancellationToken);
    }

    /// <summary>Provides a producer-consumer collection facade for a <see cref="BlockingCollection{T}"/>.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    internal sealed class ProducerConsumerWrapper<T> : IProducerConsumerCollection<T>
    {
        private readonly BlockingCollection<T> _collection;
        private readonly int _millisecondsTimeout;
        private readonly CancellationToken _cancellationToken;

        public ProducerConsumerWrapper(
            BlockingCollection<T> collection, int millisecondsTimeout, CancellationToken cancellationToken) 
        { 
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (millisecondsTimeout < -1) throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout));
            _collection = collection;
            _millisecondsTimeout = millisecondsTimeout;
            _cancellationToken = cancellationToken;
        }

        public void CopyTo(T[] array, int index)
        {
            _collection.CopyTo(array, index);
        }

        public T[] ToArray()
        {
            return _collection.ToArray();
        }

        public bool TryAdd(T item)
        {
            return _collection.TryAdd(item, _millisecondsTimeout, _cancellationToken);
        }

#nullable disable warnings // Interface is missing MaybeNullWhen attribute in .NET Framework/.NET Standard.
        public bool TryTake([MaybeNullWhen(false)] out T item)
#nullable restore warnings
        {
            return _collection.TryTake(out item, _millisecondsTimeout, _cancellationToken);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_collection).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_collection).CopyTo(array, index);
        }

        public int Count
        {
            get { return _collection.Count; }
        }

        public bool IsSynchronized
        {
            get { return ((ICollection)_collection).IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return ((ICollection)_collection).SyncRoot; }
        }
    }
}
