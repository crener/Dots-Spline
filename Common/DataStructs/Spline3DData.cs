using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Crener.Spline.Common.DataStructs
{
    /// <summary>
    /// Data required for a 3D spline along the XYZ plane.
    /// Authored using <seealso cref="ISpline3D"/> 
    /// </summary>
    public struct Spline3DData : ISharedComponentData, IEquatable<Spline3DData>, IDisposable
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
        public NativeArray<float3> Points;

        public void Dispose()
        {
            Time.Dispose();
            Points.Dispose();
        }

        public bool Equals(Spline3DData other)
        {
            return Length.Equals(other.Length) && Equals(Time, other.Time) && Equals(Points, other.Points);
        }

        public override bool Equals(object obj)
        {
            return obj is Spline3DData other && Equals(other);
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