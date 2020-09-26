using Unity.Mathematics;

namespace Crener.Spline.Common.Interfaces
{
    public interface ISpline3DEditor : ISpline3D
    {
        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>position for the point <paramref name="i"/> in local space</returns>
        float3 GetControlPoint3DLocal(int i);
        
        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <returns>position for the point <paramref name="i"/> in world space</returns>
        float3 GetControlPoint3DWorld(int i);

        /// <summary>
        /// Update an existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="point">location of the point in world space</param>
        /// <param name="mode">type of point to update</param>
        void UpdateControlPoint(int index, float3 point, SplinePoint mode);


        /// <summary>
        /// Move all points by <paramref name="delta"/> amount
        /// </summary>
        /// <param name="delta">amount to move all point by</param>
        void MoveControlPoints(float3 delta);
    }
}