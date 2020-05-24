using Crener.Spline.Test.Helpers;

namespace Crener.Spline.Test.Simple
{
    public class SharedBezierSplineTestBase : SelfCleanUpTestSet
    {
        protected BezierSpline2DJobTest.BezierSpline2DSimpleInspector CreateSpline()
        {
            BezierSpline2DJobTest.BezierSpline2DSimpleInspector spline = BezierSplineHelpers.CreateSpline();
            m_disposables.Add(spline);
            return spline;
        }
    }
}