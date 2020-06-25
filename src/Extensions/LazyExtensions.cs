//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: LazyExtensions.cs
//
//--------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace System
{
    /// <summary>Extension methods for <see cref="Lazy{T}"/>.</summary>
    public static class LazyExtensions
    {
        /// <summary>Forces value creation of a <see cref="Lazy{T}"/> instance.</summary>
        /// <typeparam name="T">Specifies the type of the value being lazily initialized.</typeparam>
        /// <param name="lazy">The <see cref="Lazy{T}"/> instance.</param>
        /// <returns>The initialized <see cref="Lazy{T}"/> instance.</returns>
        public static Lazy<T> Force<T>(this Lazy<T> lazy)
        {
            var ignored = lazy.Value;
            return lazy;
        }

        /// <summary>Retrieves the value of a <see cref="Lazy{T}"/> asynchronously.</summary>
        /// <typeparam name="T">Specifies the type of the value being lazily initialized.</typeparam>
        /// <param name="lazy">The <see cref="Lazy{T}"/> instance.</param>
        /// <returns>A <see cref="Task"/> representing the <see cref="Lazy{T}"/>'s value.</returns>
        public static Task<T> GetValueAsync<T>(this Lazy<T> lazy)
        {
            return Task.Factory.StartNew(() => lazy.Value);
        }

        /// <summary>Creates a <see cref="Lazy{T}"/> that's already been initialized to a specified <paramref name="value"/>.</summary>
        /// <typeparam name="T">The type of the data to be initialized.</typeparam>
        /// <param name="value">The value with which to initialize the <see cref="Lazy{T}"/> instance.</param>
        /// <returns>The initialized <see cref="Lazy{T}"/>.</returns>
        public static Lazy<T> Create<T>(T value)
        {
            return new Lazy<T>(() => value, false).Force();
        }
    }
}