using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._2D.LinearCubic.TestAdapters;
using Crener.Spline.Test._2D.LinearCubic.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._2D.LinearCubic
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearCubicDynamicJobTests2D : LinearCubicBaseTest2DAdapter
    {
        protected override ISimpleTestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline spline = game.AddComponent<MeaninglessTestWrapper3.TestLinearCubic2DSplineDynamicJob>();

            return spline;
        }
    }
    
    public class LoopingLinearCubicDynamicJobTests2D : BaseLoopingTests2D
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper3.TestLinearCubic2DSplineDynamicJob>();

            return spline;
        }
    }
}