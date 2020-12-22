using Crener.Spline.Test.Variance.TestAdapter;
using Crener.Spline.Test.Variance.TestTypes;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test.Variance
{
    public class BaseBezierSplineVariance2DTest2D : BaseVarianceTests2D
    {
        protected override IVarianceTestSpline2D CreateVarianceSpline()
        {
            GameObject game = new GameObject();
            IVarianceTestSpline2D spline2D = game.AddComponent<MeaninglessTestWrapper.TestBezierSpline2D2DVariance>();
            Assert.IsNotNull(spline2D);

            ClearSpline(spline2D);

            m_disposables.Add(spline2D);
            return spline2D;
        }

        protected override ISimpleTestSpline2D CreateNewSpline()
        {
            return CreateVarianceSpline();
        }
    }
}