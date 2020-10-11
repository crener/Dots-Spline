using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;

namespace Crener.Spline.Test.BaseTests.TransferableTestBases
{
    public interface ITransferableTestSet<in T> where T : ISpline
    {
        /// <summary>
        /// Abstraction of adding a point so that 2D and 3D can share the same tests
        /// </summary>
        void AddControlPointLocalSpace(T spline, float3 point);

        /// <summary>
        /// Abstraction of inserting a point so that 2D and 3D can share the same tests
        /// </summary>
        void InsertControlPointWorldSpace(T spline, int index, float3 point);

        /// <summary>
        /// Get the control point at <paramref name="index"/> from <paramref name="spline"/>
        /// </summary>
        float3 GetControlPoint(T spline, int index, SplinePoint pointType);

        /// <summary>
        /// Abstraction of updating an existing point so that 2D and 3D can share the same tests
        /// </summary>
        void UpdateControlPoint(T spline, int index, float3 newPoint, SplinePoint pointType);

        /// <summary>
        /// Abstraction of calculating a point from the spline so that 2D and 3D can share the same tests
        /// </summary>
        float3 GetProgress(T spline, float progress);

        void CompareProgressEquals(T spline, float progress, float3 expectedPoint, float tolerance = 0.00001f);
        void CompareProgressNotEquals(T spline, float progress, float3 expectedPoint);
        void ComparePoint(float3 expected, float3 actual, float tolerance = 1E-05F);

        /// <summary>
        /// Abstraction of length between two points so that 2D and 3D can share the same tests
        /// </summary>
        float Length(float3 a, float3 b);
    }

    public interface ITransferablePlainTestSet<in T> : ITransferableTestSet<T> where T : ISpline
    {
        /// <summary>
        /// Get the control point at <paramref name="index"/> from <paramref name="spline"/>
        /// </summary>
        float3 GetControlPointWorldSpace(T spline, int index, SplinePoint pointType);
    } 
}