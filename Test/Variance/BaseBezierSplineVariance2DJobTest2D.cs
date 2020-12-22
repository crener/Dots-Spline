using Crener.Spline.Test.Variance.TestAdapter;
using Crener.Spline.Test.Variance.TestTypes;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test.Variance
{
    public class BaseBezierSplineVariance2DJobTest2D : BaseVarianceTests2D
    {
        protected override IVarianceTestSpline2D CreateVarianceSpline()
        {
            GameObject game = new GameObject();
            MeaninglessTestWrapper.TestBezierSpline2D2DVariance spline = game.AddComponent<MeaninglessTestWrapper.TestBezierSpline2D2DVariance>();
            Assert.IsNotNull(spline);

            ClearSpline(spline);

            m_disposables.Add(spline);
            return spline;
        }

        protected override ISimpleTestSpline2D CreateNewSpline()
        {
            return CreateVarianceSpline();
        }
    }
}