using Crener.Spline.Common.Interfaces;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.Helpers
{
    /// <summary>
    /// abstraction which allows for easy conversion of 2D and 3D tests using a single base test set 
    /// </summary>
    public abstract class TransferableTestSet<T> : SelfCleanUpTestSet where T : ISpline 
    {
        protected abstract T CreateNewSpline();

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
        protected abstract void AddControlPoint(T spline, float3 point);

        protected void RemoveControlPoint(T spline, int index)
        {
            Assert.NotNull(spline);

            int before = spline.ControlPointCount;
            spline.RemoveControlPoint(index);

            Assert.LessOrEqual(spline.ControlPointCount, before, "Removing a point did not decrease the control point count");
        }

        protected abstract float3 GetProgress(T spline, float progress);

        protected abstract void CompareProgressEquals(T spline, float progress, float3 expectedPoint, float tolerance = 0.00001f);
        protected abstract void CompareProgress(T spline, float progress, float3 unexpectedPoint);

        /// <summary>
        /// Abstraction of length between two points so that 2D and 3D can share the same tests
        /// </summary>
        protected abstract float Length(float3 a, float3 b);
    }
}