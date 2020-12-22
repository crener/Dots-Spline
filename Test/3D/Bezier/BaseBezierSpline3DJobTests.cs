using Crener.Spline._2D.Jobs;
using Crener.Spline.Test._3D.Bezier.TestAdapters;
using Crener.Spline.Test._3D.Bezier.TestTypes;
using UnityEngine;

namespace Crener.Spline.Test._3D.Bezier
{
    /// <summary>
    /// Override for testing <see cref="BezierSpline2DPointJob"/>
    /// </summary>
    public class JobBezierSpline3D : BezierBaseTest3DAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper2.TestBezierSpline3DSimpleJob>();

            return spline;
        }
    }
}