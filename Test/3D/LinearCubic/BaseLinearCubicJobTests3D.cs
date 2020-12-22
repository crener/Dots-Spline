using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3D.LinearCubic.TestAdapters;
using Crener.Spline.Test._3D.LinearCubic.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._3D.LinearCubic
{
    /// <summary>
    /// Tests Point to point implementation of basic 3D spline functionality
    /// </summary>
    public class JobLinearCubic3D : LinearCubicBaseTest3DAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DSimpleJob>();

            return spline;
        }
    }
    
    public class JobLoopingLinearCubic3D : BaseLoopingTests3D
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DSimpleJob>();

            return spline;
        }
    }
    
    public class JobArkLinearCubic3D : BaseArkTests3D
    {
        public override IArkableSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            IArkableSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DSimpleJob>();

            return spline;
        }
    }
}