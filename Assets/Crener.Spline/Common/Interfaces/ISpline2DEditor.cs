using Unity.Mathematics;

namespace Crener.Spline.Common.Interfaces
{
    public interface ISpline2DEditor : ISpline2D
    {
        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>World Space position for the point</returns>
        float2 GetControlPoint2D(int i);

        /// <summary>
        /// Update an existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="point">location of the point</param>
        /// <param name="mode">type of point to update</param>
        void UpdateControlPoint(int index, float2 point, SplinePoint mode);


        /// <summary>
        /// Move all points by <paramref name="delta"/> amount
        /// </summary>
        /// <param name="delta">amount to move all point by</param>
        void MoveControlPoints(float2 delta);
    }
}