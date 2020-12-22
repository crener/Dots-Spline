using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3D.Linear.TestAdapters;
using Crener.Spline.Test._3D.Linear.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._3D.Linear
{
    /// <summary>
    /// Tests Point to point implementation of basic 3D spline functionality
    /// </summary>
    public class DynamicJobLinear3D : LinearBaseTest3DAdapter
    {
        public override ITestSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper3.TestLinearSpline3DDynamicJob>();

            return spline;
        }
    }
    
    public class DynamicJobLoopingLinear3D : BaseLoopingTests3D
    {
        public override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper3.TestLinearSpline3DDynamicJob>();

            return spline;
        }
    }
}