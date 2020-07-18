using Crener.Spline.Test._3D.LinearCubic.TestTypes;
using Crener.Spline.Test._3D.LinearCubic.TestAdapters;
using UnityEngine;

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
}