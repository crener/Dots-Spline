using Crener.Spline.Common;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.P2P.TestAdapters
{
    public abstract class P2PBaseTestAdapter : BaseSimpleSplineTests
    {
        [Test]
        public void NoEditModeChange([Values]SplineEditMode mode)
        {
            ISimpleTestSpline testSpline = PrepareSpline();

            float2 a = float2.zero;
            testSpline.AddControlPoint(a);

            testSpline.ChangeEditMode(0, mode);
            Assert.AreEqual(SplineEditMode.Standard, testSpline.GetEditMode(0));
        }
    }
}