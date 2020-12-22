using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Crener.Spline.Test.Misc
{
    public class TestHelperTests
    {
        [Test]
        public void _2DCompare()
        {
            float2 expected = new float2(10);
            float2 actual = new float2(10);
            
            TestHelpers.CheckFloat2(expected, actual);
        }
        
        [Test]
        public void _2DCompareNot()
        {
            float2 expected = new float2(10);
            float2 actual = new float2(100);
            
            TestHelpers.CheckNotFloat2(expected, actual);
        }
        
        [Test]
        public void _3DCompare()
        {
            float3 expected = new float3(10);
            float3 actual = new float3(10);
            
            TestHelpers.CheckFloat3(expected, actual);
        }
        
        [Test]
        public void _3DCompareNot()
        {
            float3 expected = new float3(10);
            float3 actual = new float3(100);
            
            TestHelpers.CheckNotFloat3(expected, actual);
        }
    }
}