﻿//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: ThreadSafeRandom.cs
//
//--------------------------------------------------------------------------

using System;
using System.Security.Cryptography;

namespace System.Threading
{
    /// <summary>
    /// Represents a thread-safe, pseudo-random number generator.
    /// </summary>
    public class ThreadSafeRandom : Random
    {
        /// <summary>Seed provider.</summary>
        private static readonly RNGCryptoServiceProvider _global = new RNGCryptoServiceProvider();
        /// <summary>The underlying provider of randomness, one instance per thread, initialized with <see cref="_global"/>.</summary>
        private ThreadLocal<Random> _local = new ThreadLocal<Random>(() =>
        {
            var buffer = new byte[4];
            _global.GetBytes(buffer); // RNGCryptoServiceProvider is thread-safe for use in this manner
            return new Random(BitConverter.ToInt32(buffer, 0));
        });

        /// <summary>Returns a nonnegative random number.</summary>
        /// <returns>A 32-bit signed integer greater than or equal to zero and less than <see cref="int.MaxValue"/>.</returns>
        public override int Next()
        {
            return _local.Value.Next();
        }

        /// <summary>Returns a nonnegative random number less than the specified maximum.</summary>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random number to be generated.
        /// <paramref name="maxValue"/> must be greater than or equal to zero. 
        /// </param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue"/>; 
        /// that is, the range of return values ordinarily includes zero but not <paramref name="maxValue"/>.
        /// However, if <paramref name="maxValue"/> equals zero, <paramref name="maxValue"/> is returned.
        /// </returns>
        public override int Next(int maxValue)
        {
            return _local.Value.Next(maxValue);
        }

        /// <summary>Returns a random number within a specified range.</summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must
        /// be greater than or equal to <paramref name="minValue"/>.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less
        /// than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/>
        /// but not <paramref name="maxValue"/>. If <paramref name="minValue"/> equals <paramref name="maxValue"/>,
        /// <paramref name="minValue"/> is returned.
        /// </returns>
        public override int Next(int minValue, int maxValue)
        {
            return _local.Value.Next(minValue,  maxValue);
        }

        /// <summary>Returns a random number between 0.0 and 1.0.</summary>
        /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
        public override double NextDouble()
        {
            return _local.Value.NextDouble();
        }

        /// <summary>Fills the elements of a specified array of bytes with random numbers.</summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        public override void NextBytes(byte[] buffer)
        {
            _local.Value.NextBytes(buffer);
        }
    }
}