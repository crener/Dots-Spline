using Crener.Spline.BezierSpline.Entity;
using Crener.Spline.Common.DataStructs;
using Unity.Entities;
using Unity.Mathematics;

namespace Crener.Spline.Common.Interfaces
{
    /// <summary>
    /// Shared functionality for 2D splines
    /// </summary>
    public interface ISpline2D : ISpline, IConvertGameObjectToEntity
    {
        Spline2DData? SplineEntityData { get; }
        SplineType SplineDataType { get; }

        /// <summary>
        /// Retrieve a point on the spline
        /// </summary>
        /// <param name="progress">0 to 1 range of progress along the spline</param>
        /// <returns>point on spline</returns>
        float2 GetPoint(float progress);

        /// <summary>
        /// Retrieve a point on the spline between control point index and index + 1 by progress
        /// </summary>
        /// <param name="progress">0 to 1 range of progress along the spline</param>
        /// <param name="index">index of the first control point</param>
        /// <returns>point on spline</returns>
        float2 GetPoint(float progress, int index);

        /// <summary>
        /// Adds a point to the end of the spline
        /// </summary>
        void AddControlPoint(float2 point);

        /// <summary>
        /// inserts a point before the specified segment index
        /// </summary>
        /// <param name="index">segment index</param>
        /// <param name="point">location to insert</param>
        void InsertControlPoint(int index, float2 point);
    }

    /// <summary>
    /// 2D spline
    /// </summary>
    public interface ISimpleSpline2D : ISpline2D
    {
        /// <summary>
        /// Update an existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="point">location of the point</param>
        /// <param name="mode">type of point to update</param>
        void UpdateControlPoint(int index, float2 point, SplinePoint mode);

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <param name="point">type of point to get</param>
        /// <returns>World Space position for the point</returns>
        float2 GetControlPoint(int i, SplinePoint point);
    }

    /// <summary>
    /// 2D spline with variance
    /// </summary>
    public interface ISimpleSpline2DVariance : ISpline2D
    {
        Spline2DVarianceData? SplineVarianceEntityData { get; }

        /// <summary>
        /// Length of the spline
        /// </summary>
        /// <param name="variance">-1 to 1 range of variation from the center spline</param>
        float Length(half variance);

        /// <summary>
        /// Retrieve a point on the spline with a certain amount of variance
        /// </summary>
        /// <param name="progress">0 to 1 range of progress along the spline</param>
        /// <param name="variance">-1 to 1 range of variation from the center spline</param>
        /// <returns>point on spline</returns>
        float2 GetPoint(float progress, half variance);
        
        /// <summary>
        /// Retrieve a point on the spline with a certain amount of variance
        /// </summary>
        /// <param name="progress">0 to 1 range of progress along the spline</param>
        /// <param name="index">index of the first control point</param>
        /// <param name="variance">-1 to 1 range of variation from the center spline</param>
        /// <returns>point on spline</returns>
        float2 GetPoint(float progress, int index, half variance);

        /// <summary>
        /// Update an existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="point">location of the point</param>
        /// <param name="mode">type of point to update</param>
        void UpdateControlPoint(int index, float2 point, SplinePointVariance mode);

        /// <summary>
        /// Gets the given point from a point segment
        /// </summary>
        /// <param name="i">index of the segment</param>
        /// <param name="point">type of point to get</param>
        /// <returns>World Space position for the point</returns>
        float2 GetControlPoint(int i, SplinePointVariance point);
    }
}