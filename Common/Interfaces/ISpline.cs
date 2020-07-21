using System;

namespace Crener.Spline.Common.Interfaces
{
    /// <summary>
    /// basic spline behaviour
    /// </summary>
    public interface ISpline : IDisposable
    {
        /// <summary>
        /// Amount of control points in the spline
        /// </summary>
        int ControlPointCount { get; }
        
        /// <summary>
        /// Amount of points in the spline that can be interpolated
        /// </summary>
        /// <remarks>for most spline this will be equal to <see cref="ControlPointCount"/> unless it's looped</remarks>
        int SegmentPointCount { get; }

        /// <summary>
        /// Length of the spline
        /// </summary>
        float Length();
        
        /// <summary>
        /// Cleans up the spline data by disposing any disposable types 
        /// </summary>
        void ClearData();
        
        /// <summary>
        /// Remove existing control points data
        /// </summary>
        /// <param name="index">control point index</param>
        void RemoveControlPoint(int index);
        
        /// <summary>
        /// Get the edit mode for a control point 
        /// </summary>
        /// <param name="index"> control point index</param>
        /// <returns>edit mode for the control point</returns>
        SplineEditMode GetEditMode(int index);

        /// <summary>
        /// Change the edit mode of a control point
        /// </summary>
        /// <param name="index">control point index</param>
        /// <param name="mode">new control point edit mode</param>
        void ChangeEditMode(int index, SplineEditMode mode);
    }
}