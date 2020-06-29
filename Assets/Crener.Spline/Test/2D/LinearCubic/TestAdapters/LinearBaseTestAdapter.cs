using Crener.Spline.Common;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test._2D.LinearCubic.TestAdapters
{
    public abstract class LinearCubicBaseTestAdapter : BaseSimpleSplineTests
    {
        // todo add tests which compare a 4 point square with a perfect circle 
        
        // todo add tests which check the error range in the first segment and last segment
        // once the weight has been removed from those first points
        
        //todo test that 3 point spline doesn't touch 2nd point when in L shape
        //todo test that 3 point spline without looping doesn't end at the first point
        
        //todo test that 4 point spline doesn't touch 2nd and 3rd point
        
        //todo test that 3 or 4 point spline doesn't touch any point directly
    }
}