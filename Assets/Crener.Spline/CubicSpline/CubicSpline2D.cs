using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.CubicSpline
{
    /// <summary>
    /// Simple spline which directly follows a set of points
    /// </summary>
    public class CubicSpline2D : BaseSpline2D, ILoopingSpline
    {
        [SerializeField]
        private bool looped = false;
        [SerializeField]
        private int smoothing = 2;

        public bool Looped
        {
            get => looped && ControlPointCount > 2;
            set
            {
                looped = value;
                RecalculateLengthBias();
            }
        }
        public override SplineType SplineDataType => SplineType.Cubic;

        public override int SegmentPointCount => Looped ? ControlPointCount + 1 : ControlPointCount;

        const int c_precesion = 20;

        private CubicSpline m_spline;
        //private float2[] Interpolated;
        //private Matrix m_matrix;
        //private float[] a, b, c, d, h;

        public override float2 GetPoint(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(ControlPointCount == 1 || progress <= 0f)
                return GetControlPoint(0);
            else if(ControlPointCount == 2)
                return math.lerp(GetControlPoint(0), GetControlPoint(1), progress);
            else if(ControlPointCount == 3)
                return Cubic3Point(0, 1, 2, progress);
            else if(progress <= 0f)
                return GetControlPoint(0);
            else if(progress >= 1f)
                return GetControlPoint(ControlPointCount - 1);

            int index = (int) ((c_precesion * (SegmentPointCount - 1)) * progress);
            return m_spline.Interpolated[index];

            //int aIndex = FindSegmentIndex(progress);
            //float pointProgress = SegmentProgress(progress, aIndex);
            //return SplineInterpolation(pointProgress, aIndex);
        }

        protected override void RecalculateLengthBias()
        {
            ClearData();
            SegmentLength.Clear();
            m_spline = new CubicSpline(Points.ToArray(), c_precesion, smoothing);
            //CalculateCubicParameters();

            if(ControlPointCount <= 1)
            {
                LengthCache = 0f;
                SegmentLength.Add(1f);
                return;
            }

            if(ControlPointCount == 2)
            {
                LengthCache = LengthBetweenPoints(0, 128);
                SegmentLength.Add(1f);
                return;
            }

            // calculate the distance that the entire spline covers
            float currentLength = 0f;
            for (int a = 0; a < SegmentPointCount - 1; a++)
            {
                float length = LengthBetweenPoints(a, 128);
                currentLength += length;
            }

            LengthCache = currentLength;

            if(SegmentPointCount == 2)
            {
                SegmentLength.Add(1f);
                return;
            }

            // calculate the distance that a single segment covers
            float segmentCount = 0f;
            for (int a = 0; a < SegmentPointCount - 1; a++)
            {
                float length = LengthBetweenPoints(a, 128);
                segmentCount = (length / LengthCache) + segmentCount;
                SegmentLength.Add(segmentCount);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int a)
        {
            int index = (int) ((c_precesion * (SegmentPointCount - 1)) * t);
            return m_spline.Interpolated[index];
        }

        private float2 Cubic3Point(int a, int b, int c, float t)
        {
            float2 p1 = Points[a];
            float2 p2 = Points[b];
            float2 p3 = Points[c];

            float2 i0 = math.lerp(p1, p2, t);
            float2 i1 = math.lerp(p2, p3, t);

            return math.lerp(i0, i1, t);
        }

        /*private void CalculateCubicParameters()
        {
            if(ControlPointCount < 4)
            {
                // not enough data to correctly initialize the cubic stuff so just try to save some memory...
                Interpolated = new float2[0];
                m_matrix = new Matrix(0);

                a = new float[Points.Count];
                b = new float[Points.Count];
                c = new float[Points.Count];
                d = new float[Points.Count];
                h = new float[Points.Count - 1];
            }
            else
            {
                Interpolated = new float2[Points.Count * c_precesion];
                m_matrix = new Matrix(Points.Count - 1, smoothing);

                a = new float[Points.Count];
                b = new float[Points.Count];
                c = new float[Points.Count];
                d = new float[Points.Count];
                h = new float[Points.Count - 1];

                CalcParameters();
                Interpolate();
            }
        }
        
        private void CalcParameters()
        {
            for (int i = 0; i < Points.Count; i++)
                a[i] = Points[i].y;

            for (int i = 0; i < Points.Count - 1; i++)
                h[i] = Points[i + 1].x - Points[i].x;

            for (int i = 0; i < Points.Count - 2; i++)
            {
                for (int k = 0; k < Points.Count - 2; k++)
                {
                    m_matrix.a[i, k] = 0;
                    m_matrix.y[i] = 0;
                    m_matrix.x[i] = 0;
                }
            }

            for (int i = 0; i < Points.Count - 2; i++)
            {
                if(i == 0)
                {
                    m_matrix.a[i, 0] = 2f * (h[0] + h[1]);
                    m_matrix.a[i, 1] = h[1];
                }
                else
                {
                    m_matrix.a[i, i - 1] = h[i];
                    m_matrix.a[i, i] = 2f * (h[i] + h[i + 1]);
                    if(i < Points.Count - 3)
                        m_matrix.a[i, i + 1] = h[i + 1];
                }

                if((h[i] != 0) && (h[i + 1] != 0))
                    m_matrix.y[i] = ((a[i + 2] - a[i + 1]) / h[i + 1] - (a[i + 1] - a[i]) / h[i]) * 3f;
                else
                    m_matrix.y[i] = 0f;
            }

            if(m_matrix.Eliminate() == false)
                throw new InvalidOperationException("error in matrix calculation");

            m_matrix.Solve();

            c[0] = 0f;
            c[Points.Count - 1] = 0f;

            for (int i = 1; i < Points.Count - 1; i++)
                c[i] = m_matrix.x[i - 1];

            for (int i = 0; i < Points.Count - 1; i++)
            {
                if(h[i] != 0.0)
                {
                    d[i] = 1f / 3f / h[i] * (c[i + 1] - c[i]);
                    b[i] = 1f / h[i] * (a[i + 1] - a[i]) - h[i] / 3f * (c[i + 1] + 2f * c[i]);
                }
            }
        }

        private void Interpolate()
        {
            int resolution = Interpolated.Length / Points.Count;
            for (int i = 0; i < h.Length; i++)
            {
                for (int k = 0; k < resolution; k++)
                {
                    float deltaX = (float) k / resolution * h[i];
                    float termA = a[i];
                    float termB = b[i] * deltaX;
                    float termC = c[i] * deltaX * deltaX;
                    float termD = d[i] * deltaX * deltaX * deltaX;
                    int interpolatedIndex = i * resolution + k;
                    Interpolated[interpolatedIndex] = new float2(deltaX + Points[i].x, termA + termB + termC + termD);
                }
            }

            // After interpolation the last several values of the interpolated arrays
            // contain uninitialized data. This section identifies the values which are
            // populated with values and copies just the useful data into new arrays.
            int pointsToKeep = resolution * (Points.Count - 1) + 1;
            float2[] interpolatedCopy = new float2[pointsToKeep];
            Array.Copy(Interpolated, 0, interpolatedCopy, 0, pointsToKeep - 1);
            Interpolated = interpolatedCopy;
            Interpolated[pointsToKeep - 1] = Points[Points.Count - 1];
        }*/

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            return;

            //todo implement this for b-spline
            /*dstManager.AddComponent<Spline2DData>(entity);
            Spline2DData splineData = ConvertData();
            SplineEntityData = splineData;
            dstManager.SetSharedComponentData(entity, splineData);*/
        }

        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            const float pointDensity = 13;

            if(SegmentPointCount > 0 && SegmentLength.Count == 0 || m_spline == null)
            {
                // needs to calculate length as it might not have been saved correctly after saving
                RecalculateLengthBias();
            }

            for (int i = 0; i < ControlPointCount - 1; i++)
            {
                float2 f = GetPoint(0f, i);
                Vector3 lp = new Vector3(f.x, f.y, 0f);
                int points = (int) (pointDensity * (SegmentLength[i] * Length()));

                for (int s = 0; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float2 p = GetPoint(progress, i);
                    Vector3 point = new Vector3(p.x, p.y, 0f);

                    Gizmos.DrawLine(lp, point);
                    lp = point;
                }
            }
        }
    }
}