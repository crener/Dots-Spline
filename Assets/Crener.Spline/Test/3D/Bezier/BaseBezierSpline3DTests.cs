using Crener.Spline._2D;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3D.Bezier.TestAdapters;
using Crener.Spline.Test._3D.Bezier.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._3D.Bezier
{
    /// <summary>
    /// Override for testing <see cref="BezierSpline2DSimple"/>
    /// </summary>
    public class BaseBezierSpline3D : BezierBaseTest3DAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper.TestBezierSpline3DSimple>();

            return spline;
        }
    }
    
    public class BaseArkBezierSpline3D : BaseArkTests3D
    {
        public override IArkableSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            MeaninglessTestWrapper.TestBezierSpline3DSimple testBezierSpline = game.AddComponent<MeaninglessTestWrapper.TestBezierSpline3DSimple>();

            return testBezierSpline;
        }
    }
}