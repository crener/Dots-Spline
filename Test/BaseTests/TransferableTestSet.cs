using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.BaseTests.TransferableTestBases;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.BaseTests
{
    /// <summary>
    /// abstraction which allows for easy conversion of 2D and 3D tests using a single base test set 
    /// </summary>
    public abstract class TransferableTestSet<T> : SelfCleanUpTestSet, ITransferableTestSet<T> where T : ISpline 
    {
        public abstract T CreateNewSpline();

        /// <summary>
        /// Create a new spline and validates that it is ready for testing
        /// </summary>
        protected virtual T PrepareSpline()
        {
            T spline = CreateNewSpline();
            Assert.IsNotNull(spline);

            TestHelpers.ClearSpline(spline);
            m_disposables.Add(spline);
            
            return spline;
        }

        /// <summary>
        /// Abstraction of adding a point so that 2D and 3D can share the same tests
        /// </summary>
        public abstract void AddControlPointLocalSpace(T spline, float3 point);

        /// <summary>
        /// Abstraction of inserting a point from a location in world space so that 2D and 3D can share the same tests
        /// </summary>
        public abstract void InsertControlPointWorldSpace(T spline, int index, float3 point);

        /// <summary>
        /// Abstraction of inserting a point from a location in local space so that 2D and 3D can share the same tests
        /// </summary>
        public abstract void InsertControlPointLocalSpace(T spline, int index, float3 point);

        /// <summary>
        /// Get the control point at <paramref name="index"/> from <paramref name="spline"/>
        /// </summary>
        public abstract float3 GetControlPoint(T spline, int index, SplinePoint pointType);

        /// <summary>
        /// Abstraction of updating an existing point so that 2D and 3D can share the same tests
        /// </summary>
        public abstract void UpdateControlPoint(T spline, int index, float3 newPoint, SplinePoint pointType);
        
        protected void RemoveControlPoint(T spline, int index)
        {
            Assert.NotNull(spline);

            int before = spline.ControlPointCount;
            spline.RemoveControlPoint(index);

            Assert.LessOrEqual(spline.ControlPointCount, before, "Removing a point did not decrease the control point count");
        }

        /// <summary>
        /// Abstraction of calculating a point from the spline so that 2D and 3D can share the same tests
        /// </summary>
        public abstract float3 GetProgressWorld(T spline, float progress);

        /// <summary>
        /// Abstraction of calculating a point from the spline so that 2D and 3D can share the same tests
        /// </summary>
        public abstract float3 GetProgressLocal(T spline, float progress);

        public abstract void CompareProgressEquals(T spline, float progress, float3 expectedPoint, float tolerance = 0.00001f);
        public abstract void CompareProgressNotEquals(T spline, float progress, float3 expectedPoint);
        public abstract void ComparePoint(float3 expected, float3 actual, float tolerance = 1E-05F);

        /// <summary>
        /// Abstraction of length between two points so that 2D and 3D can share the same tests
        /// </summary>
        public abstract float Length(float3 a, float3 b);
    }
}