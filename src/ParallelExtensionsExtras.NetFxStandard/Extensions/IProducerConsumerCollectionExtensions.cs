﻿//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: IProducerConsumerCollectionExtensions.cs
//
//--------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Concurrent;

/// <summary>Extension methods for <see cref="IProducerConsumerCollection{T}"/>.</summary>
public static class ProducerConsumerCollectionExtensions
{
    /// <summary>Clears the <paramref name="collection"/> by repeatedly taking elements until it's empty.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection to be cleared.</param>
    public static void Clear<T>(this IProducerConsumerCollection<T> collection)
    {
        while (collection.TryTake(out _));
    }

    /// <summary>Creates an enumerable which will consume and return elements from the <paramref name="collection"/>.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection to be consumed.</param>
    /// <returns>An enumerable that consumes elements from the <paramref name="collection"/> and returns them.</returns>
    public static IEnumerable<T> GetConsumingEnumerable<T>(
        this IProducerConsumerCollection<T> collection)
    {
        while (collection.TryTake(out var item)) yield return item;
    }

    /// <summary>Adds the contents of a <paramref name="source"/> enumerable to the <paramref name="target"/> collection.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="target">The target collection to be augmented.</param>
    /// <param name="source">The source enumerable containing the data to be added.</param>
    public static void AddFromEnumerable<T>(this IProducerConsumerCollection<T> target, IEnumerable<T> source)
    {
        foreach (var item in source) target.TryAdd(item);
    }

    /// <summary>Adds the contents of a <paramref name="source"/> observable to the <paramref name="target"/> collection.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="target">The target collection to be augmented.</param>
    /// <param name="source">The source observable containing the data to be added.</param>
    /// <returns>An <see cref="IDisposable"/> that may be used to cancel the transfer.</returns>
    public static IDisposable AddFromObservable<T>(this IProducerConsumerCollection<T> target, IObservable<T> source)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (source == null) throw new ArgumentNullException(nameof(source));
        return source.Subscribe(new DelegateBasedObserver<T>
        (
            onNext: item => target.TryAdd(item),
            onError: error => {},
            onCompleted: () => {}
        ));
    }

    /// <summary>Creates an add-only facade for the <paramref name="collection"/>.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection to be wrapped.</param>
    /// <returns>
    /// An <see cref="IProducerConsumerCollection{T}"/> that wraps the <paramref name="collection"/> and supports only add
    /// functionality, not take.
    /// </returns>
    public static IProducerConsumerCollection<T> ToProducerOnlyCollection<T>(this IProducerConsumerCollection<T> collection)
    {
        return new ProduceOrConsumeOnlyCollection<T>(collection, true);
    }

    /// <summary>Creates a take-only facade for the <paramref name="collection"/>.</summary>
    /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
    /// <param name="collection">The collection to be wrapped.</param>
    /// <returns>
    /// An <see cref="IProducerConsumerCollection{T}"/> that wraps the <paramref name="collection"/> and supports only take
    /// functionality, not add.
    /// </returns>
    public static IProducerConsumerCollection<T> ToConsumerOnlyCollection<T>(this IProducerConsumerCollection<T> collection)
    {
        return new ProduceOrConsumeOnlyCollection<T>(collection, false);
    }

    // Internal wrapper that throws NotSupportedException when mutating methods (add/take) are used from the wrong mode
    private sealed class ProduceOrConsumeOnlyCollection<T> : ProducerConsumerCollectionBase<T>
    {
        private readonly bool _produceOnly; // true for produce-only, false for consume-only

        public ProduceOrConsumeOnlyCollection(IProducerConsumerCollection<T> contained, bool produceOnly) :
            base(contained)
        {
            _produceOnly = produceOnly;
        }

        protected override bool TryAdd(T item)
        {
            if (!_produceOnly) throw new NotSupportedException();
            return base.TryAdd(item);
        }

        protected override bool TryTake([MaybeNullWhen(false)] out T item)
        {
            if (_produceOnly) throw new NotSupportedException();
            return base.TryTake(out item);
        }
    }
}
