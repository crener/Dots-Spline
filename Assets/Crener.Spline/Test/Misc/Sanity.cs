using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Crener.Spline.Test.Misc
{
    /// <summary>
    /// Tests to help make sure that basic functions work as expected so we know if something out of our control is broken
    /// </summary>
    public class Sanity
    {
        [Test]
        public void QuaternionUp() => Quaternion(new float3(0f,10f, 0f));
        [Test]
        public void QuaternionRight() => Quaternion(new float3(10f,0f, 0f));
        [Test]
        public void QuaternionForward() => Quaternion(new float3(0f,0f, 10f));

        private void Quaternion(float3 pos)
        {
            Quaternion oldQuaternion = new Quaternion(0f, 0.7f, 0f, 0.7f);
            quaternion newQuaternion = new quaternion(0f, 0.7f, 0f, 0.7f);
            TestHelpers.CheckFloat3((float3)(oldQuaternion * pos), math.mul(newQuaternion, pos));
        }
    }
}