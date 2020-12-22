using Crener.Spline.Test._3D.Bezier.TestAdapters;
using Crener.Spline.Test._3D.Bezier.TestTypes;
using UnityEngine;

namespace Crener.Spline.Test._3D.Bezier
{
    /// <summary>
    /// Tests Point to point implementation of basic 3D spline functionality
    /// </summary>
    public class DynamicJobBezierSpline3D : BezierBaseTest3DAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper3.TestBezierSpline3DDynamicJob>();

            return spline;
        }
    }
    
    /*
    public class DynamicJobLoopingBezierSpline3D : BaseLoopingTests3D
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper3.TestBezierSpline3DDynamicJob>();

            return spline;
        }
    }*/
}