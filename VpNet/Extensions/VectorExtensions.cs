﻿using System.Numerics;

namespace VpNet.Extensions
{
    internal static class VectorExtensions
    {
        /// <summary>
        ///     Deconstructs this vector.
        /// </summary>
        /// <param name="vector">The vector to deconstruct.</param>
        /// <param name="x">The X component value.</param>
        /// <param name="y">The Y component value.</param>
        /// <param name="z">The Z component value.</param>
        public static void Deconstruct(this Vector3 vector, out float x, out float y, out float z)
        {
            x = vector.X;
            y = vector.Y;
            z = vector.Z;
        }
    }
}
