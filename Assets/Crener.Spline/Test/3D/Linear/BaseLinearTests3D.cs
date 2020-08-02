using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test._3D.Linear.TestAdapters;
using Crener.Spline.Test._3D.Linear.TestTypes;
using Crener.Spline.Test.BaseTests;
using UnityEngine;

namespace Crener.Spline.Test._3D.Linear
{
    /// <summary>
    /// Tests Point to point implementation of basic 2D spline functionality
    /// </summary>
    public class BaseLinearTests3D : LinearBaseTest3DAdapter
    {
        protected override ISimpleTestSpline3D CreateNewSpline()
        {
            GameObject game = new GameObject();
            ISimpleTestSpline3D spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline3DSimple>();

            return spline;
        }
    }
    
    public class LoopingLinearTests3D : BaseLoopingTests3D
    {
        protected override ILoopingSpline CreateNewSpline()
        {
            GameObject game = new GameObject();
            ILoopingSpline spline = game.AddComponent<MeaninglessTestWrapper.TestLinearSpline3DSimple>();

            return spline;
        }
    }
}