using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Test.BaseTests.TransferableTestBases;
using Crener.Spline.Test.Helpers;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Test._3DPlane
{
    public abstract class Base3DPlaneTests : SelfCleanUpTestSet
    {
        /// <summary>
        /// Create a new instance of the spline
        /// </summary>
        protected abstract ISpline3DPlaneEditor CreateNewSpline();

        /// <summary>
        /// Create a new spline and validates that it is ready for testing
        /// </summary>
        protected ISpline3DPlaneEditor PrepareSpline()
        {
            ISpline3DPlaneEditor spline = CreateNewSpline();
            Assert.IsNotNull(spline);

            TestHelpers.ClearSpline(spline);
            m_disposables.Add(spline);

            return spline;
        }

        [Test]
        public void GameobjectTranslation([NUnit.Framework.Range(1, 50, 5)] int magnitude)
        {
            ISpline3DPlane spline = PrepareSpline();
            Assume.That(spline is MonoBehaviour, "Test isn't valid for this spline");

            float3 a = new float3(0f, 0f, 0f);
            spline.AddControlPoint(a);
            float3 b = new float3(10f, 0f, 0f);
            spline.AddControlPoint(b);

            Assert.AreEqual(2, spline.ControlPointCount);

            // make sure everything is where it's supposed to be
            TestHelpers.CheckFloat3(a, spline.Get3DPoint(0f));
            TestHelpers.CheckFloat2(a.xy, spline.Get2DPoint(0f));
            TestHelpers.CheckFloat3(b, spline.Get3DPoint(1f));
            TestHelpers.CheckFloat2(b.xy, spline.Get2DPoint(1f));

            // move the gameobject
            float3 pos = Vector3.one * magnitude;
            MoveGameobject(spline, pos);

            // check that the spline has moved accordingly
            TestHelpers.CheckFloat3(a + pos, spline.Get3DPoint(0f));
            TestHelpers.CheckFloat3(b + pos, spline.Get3DPoint(1f));
        }

        [Test]
        public void Rotation()
        {
            ISpline3DPlane spline = PrepareSpline();

            float3 a = new float3(10f, 5f, 0f);
            spline.AddControlPoint(a);
            TestHelpers.CheckFloat3(a, spline.Get3DPoint(0f));
            TestHelpers.CheckFloat2(a.xy, spline.Get2DPoint(0f));

            {
                // rotate a little bit
                Quaternion targetRotation = Quaternion.Euler(0f, 90f, 0f);
                spline.Forward = targetRotation;

                TestHelpers.CheckFloat3(new float3(0f, 5f, -10f), spline.Get3DPoint(0f));
                TestHelpers.CheckFloat2(a.xy, spline.Get2DPoint(0f));
            }
            {
                // Go back to the original orientation
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
                spline.Forward = targetRotation;

                TestHelpers.CheckFloat3(a, spline.Get3DPoint(0f));
                TestHelpers.CheckFloat2(a.xy, spline.Get2DPoint(0f));
            }
        }

        [Test]
        public void RotationNoMove()
        {
            ISpline3DPlaneEditor spline = PrepareSpline();

            float3 a = new float3(10f, 5f, 0f);
            spline.AddControlPoint(a);
            TestHelpers.CheckFloat3(a, spline.GetControlPoint3D(0));
            TestHelpers.CheckFloat2(a.xy, spline.GetControlPoint2D(0));

            // rotate
            Quaternion targetRotation = Quaternion.Euler(0f, 90f, 0f);
            spline.Forward = targetRotation;
            float3 rotatedPoint = targetRotation * a;
            TestHelpers.CheckFloat3(rotatedPoint, spline.GetControlPoint3D(0));
            TestHelpers.CheckFloat2(a.xy, spline.GetControlPoint2D(0));

            // update the cp position along the axis parallel to the plain
            float3 perpendicularPlain = new float3(10f, 0f, 0f);
            float3 newWorldLocation = new float3(0f, 5f, -10f);
            spline.UpdateControlPoint(0, newWorldLocation + perpendicularPlain, SplinePoint.Point);
            TestHelpers.CheckFloat3(newWorldLocation, spline.GetControlPoint3D(0));
            TestHelpers.CheckFloat2(a.xy, spline.GetControlPoint2D(0));

            // undo movement and check that the point is where we expect
            spline.Forward = Quaternion.identity;
            MoveGameobject(spline, new float3(0f));
            TestHelpers.CheckFloat3(a, spline.GetControlPoint3D(0));
            TestHelpers.CheckFloat2(a.xy, spline.GetControlPoint2D(0));
        }

        [Test]
        public void RotationMove()
        {
            ISpline3DPlaneEditor spline = PrepareSpline();

            float3 a = new float3(10f, 5f, 0f);
            spline.AddControlPoint(a);
            TestHelpers.CheckFloat3(a, spline.GetControlPoint3D(0));
            TestHelpers.CheckFloat2(a.xy, spline.GetControlPoint2D(0));

            // rotate
            Quaternion targetRotation = Quaternion.Euler(0f, 90f, 0f);
            spline.Forward = targetRotation;
            float3 rotatedPoint = targetRotation * a;
            TestHelpers.CheckFloat3(rotatedPoint, spline.GetControlPoint3D(0));
            TestHelpers.CheckFloat2(a.xy, spline.GetControlPoint2D(0));

            // update the cp position along the axis parallel to the plain
            float3 newWorldLocation = new float3(0f, 5f, -10f);
            TestHelpers.CheckFloat3(newWorldLocation, spline.GetControlPoint3D(0));
            newWorldLocation += new float3(0f, 0f, 5f);
            spline.UpdateControlPoint(0, newWorldLocation, SplinePoint.Point);
            TestHelpers.CheckFloat3(newWorldLocation, spline.GetControlPoint3D(0));
            
            a = new float3(5f, 5f, 0f);
            TestHelpers.CheckFloat2(a.xy, spline.GetControlPoint2D(0));

            // undo movement and check that the point is where we expect
            spline.Forward = Quaternion.identity;
            MoveGameobject(spline, new float3(0f));
            TestHelpers.CheckFloat3(a, spline.GetControlPoint3D(0));
            TestHelpers.CheckFloat2(a.xy, spline.GetControlPoint2D(0));
        }

        [Test]
        public void Conversion()
        {
            ISpline3DPlaneEditor spline = PrepareSpline();

            float3 a = new float3(10f, 5f, 0f);
            spline.AddControlPoint(a);
            TestHelpers.CheckFloat3(a, spline.GetControlPoint3D(0));
            TestHelpers.CheckFloat2(a.xy, spline.GetControlPoint2D(0));
        }

        [Test]
        public void ConversionMove()
        {
            ISpline3DPlaneEditor spline = PrepareSpline();

            float3 a = new float3(10f, 5f, 0f);
            spline.AddControlPoint(a);
            TestHelpers.CheckFloat3(a, spline.GetControlPoint3D(0));

            float3 origin = new float3(0f, 0f, 10f);
            MoveGameobject(spline, origin);
            TestHelpers.CheckFloat3(a + origin, spline.GetControlPoint3D(0));

            float3 additionalTranslation = a + origin + new float3(1f, 1f, 0f);
            spline.UpdateControlPoint(0, additionalTranslation, SplinePoint.Point);
            TestHelpers.CheckFloat3(additionalTranslation, spline.GetControlPoint3D(0));
            TestHelpers.CheckFloat2(additionalTranslation.xy, spline.GetControlPoint2D(0));
        }

        private void MoveGameobject(ISpline3DPlane spline, float3 pos)
        {
            Transform trans = (spline as MonoBehaviour)?.transform;
            Assert.IsNotNull(trans);

            trans.position = pos;
        }
    }
}