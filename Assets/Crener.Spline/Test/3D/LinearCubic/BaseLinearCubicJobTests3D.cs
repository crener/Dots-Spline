using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3D.LinearCubic.TestTypes;
using Crener.Spline.Test._3D.LinearCubic.TestAdapters;
using Crener.Spline.Test.BaseTests;
using UnityEngine;
using MeaninglessTestWrapper = Crener.Spline.Test._3D.Linear.TestTypes.MeaninglessTestWrapper;

namespace Crener.Spline.Test._3D.LinearCubic
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearCubicJobTests3D : LinearCubicBaseTest3DAdapter
    {
        protected override ISimpleTestSpline3D CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DSimpleJob>();

            return spline;
        }
    }
    
    public class LoopingLinearCubicJobTests3D : BaseLoopingTests3D
    {
        protected override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper2.TestLinearCubicSpline3DSimpleJob>();

            return spline;
        }
    }
}