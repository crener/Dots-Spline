using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Crener.Spline.BezierSpline.Entity
{
    /// <summary>
    /// Data required for a 2D spline with variance
    /// </summary>
    /// <remarks>
    /// <see cref="Spline2DVarianceTraverserSystem.OnDestory"/> handles cleanup of the data for this component
    /// </remarks>
    [DebuggerDisplay("Spline Variance Data CP: {ControlPointCount}, Length: {Length[0]:N3}")]
    public struct Spline2DVarianceData : ISharedComponentData, IEquatable<Spline2DVarianceData>, IDisposable
    {
        /// <summary>
        /// Length of the spline in world units
        /// </summary>
        [ReadOnly]
        public NativeArray<float> Length;

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
        
        /// <summary>
        /// Amount of control points in this spline
        /// </summary>
        [ReadOnly]
        public int ControlPointCount;

        public void Dispose()
        {
            Length.Dispose();
            Time.Dispose();
            Points.Dispose();
        }

        public bool Equals(Spline2DVarianceData other)
        {
            return Length.Equals(other.Length) && 
                   Equals(Time, other.Time) && 
                   Equals(Points, other.Points);
        }

        public override bool Equals(object obj)
        {
            return obj is Spline2DVarianceData other && Equals(other);
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