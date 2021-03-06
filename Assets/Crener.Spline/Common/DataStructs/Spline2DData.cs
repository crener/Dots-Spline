using System;
using Crener.Spline.Common.Interfaces;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Crener.Spline.Common.DataStructs
{
    /// <summary>
    /// Data required for a 2D spline along the XY plane.
    /// Authored using <seealso cref="ISpline2D"/> 
    /// </summary>
    public struct Spline2DData : ISharedComponentData, IEquatable<Spline2DData>, IDisposable
    {
        /// <summary>
        /// Length of the spline in world units
        /// </summary>
        [ReadOnly]
        public float Length;

        /// <summary>
        /// Progress that each point covers along the spline for working out the range that a point is responsible for
        /// </summary>
        [ReadOnly]
        public NativeArray<float> Time;

        /// <summary>
        /// Points that make up the spline
        /// </summary>
        [ReadOnly]
        public NativeArray<float2> Points;

        public void Dispose()
        {
            Time.Dispose();
            Points.Dispose();
        }

        public bool Equals(Spline2DData other)
        {
            return Length.Equals(other.Length) && Equals(Time, other.Time) && Equals(Points, other.Points);
        }

        public override bool Equals(object obj)
        {
            return obj is Spline2DData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Length.GetHashCode();
                hashCode = (hashCode * 397) ^ (Time != null ? Time.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Points != null ? Points.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}