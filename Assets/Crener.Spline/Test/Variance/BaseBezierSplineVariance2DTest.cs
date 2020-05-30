using Crener.Spline.Test.Variance.TestAdapter;
using Crener.Spline.Test.Variance.TestTypes;
using NUnit.Framework;
using UnityEngine;

namespace Crener.Spline.Test.Variance
{
    public class BaseBezierSplineVariance2DTest : BaseVarianceTests
    {
        protected override IVarianceTestSpline CreateVarianceSpline()
        {
            GameObject game = new GameObject();
            IVarianceTestSpline spline = game.AddComponent<MeaninglessTestWrapper.TestBezierSpline2DVariance>();
            Assert.IsNotNull(spline);

            ClearSpline(spline);

            m_disposables.Add(spline);
            return spline;
        }

        protected override ISimpleTestSpline CreateNewSpline()
        {
            return CreateVarianceSpline();
        }
    }
}