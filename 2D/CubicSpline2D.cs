using System;
using System.Runtime.CompilerServices;
using Crener.Spline.BaseSpline;
using Crener.Spline.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline._2D
{
    /// <summary>
    /// Simple spline which directly follows a set of points with cubic interpolation
    /// </summary>
    [AddComponentMenu("Spline/2D/Cubic Spline 2D")]
    public class CubicSpline2D : BaseSpline2D
    {
        public override SplineType SplineDataType
        {
            get
            {
                if(ControlPointCount == 0) return SplineType.Empty;
                if(ControlPointCount == 1) return SplineType.Single;
                if(ControlPointCount == 2) return SplineType.Linear;
                //if(ControlPointCount == 3) return SplineType.CubicLinear3; // todo implement maybe?
                return SplineType.Cubic;
            }
        }

        private Matrix m_matrix;
        [SerializeField]
        private float[] a, b, c, d, segmentDistance;

        protected override bool DataInitialized => segmentDistance != null && base.DataInitialized;

        public override float2 Get2DPointLocal(float progress)
        {
            if(ControlPointCount == 0)
                return float2.zero;
            else if(progress <= 0f)
                return GetControlPoint2DLocal(0);
            else if(progress >= 1f)
                return GetControlPoint2DLocal(ControlPointCount - 1);
            else if(ControlPointCount == 1 || progress <= 0f)
                return GetControlPoint2DLocal(0);
            else if(ControlPointCount == 2)
                return math.lerp(GetControlPoint2DLocal(0), GetControlPoint2DLocal(1), progress);
            else if(ControlPointCount == 3)
                return Cubic3Point(0, 1, 2, progress);

            int aIndex = FindSegmentIndex(progress);
            float pointProgress = SegmentProgress(progress, aIndex);
            return SplineInterpolation(pointProgress, aIndex);
        }

        /// <inheritdoc cref="BaseSpline2D.MoveControlPoints(float2)"/>
        public override void MoveControlPoints(float2 delta)
        {
            base.MoveControlPoints(delta);
            RecalculateLengthBias();
        }
        
        protected override void RecalculateLengthBias()
        {
            const int res = LengthSampleCount;

            ClearData();
            SegmentLength.Clear();
            CalculateCubicParameters();

            if(ControlPointCount <= 1)
            {
                LengthCache = 0f;
                SegmentLength.Add(1f);
                return;
            }

            if(ControlPointCount == 2)
            {
                LengthCache = math.distance(GetControlPoint2DLocal(0), GetControlPoint2DLocal(1));
                SegmentLength.Add(1f);
                return;
            }

            if(ControlPointCount == 3)
            {
                LengthCache = LengthBetweenPoints(0, res);
                SegmentLength.Add(1f);
                return;
            }

            // fallback to known good code
            base.RecalculateLengthBias();
        }

        protected override float LengthBetweenPoints(int a, int resolution = 64)
        {
            if(ControlPointCount == 3)
            {
                float currentLength = 0;

                float2 aPoint = Cubic3Point(0, 1, 2, 0f);
                for (float i = 1; i <= resolution; i++)
                {
                    float2 bPoint = Cubic3Point(0, 1, 2, i / resolution);
                    currentLength += math.distance(aPoint, bPoint);
                    aPoint = bPoint;
                }

                return currentLength;
            }

            return base.LengthBetweenPoints(a, resolution);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float2 SplineInterpolation(float t, int segment)
        {
            float deltaX = t * segmentDistance[segment];
            float termA = a[segment];
            float termB = b[segment] * deltaX;
            float termC = c[segment] * deltaX * deltaX;
            float termD = d[segment] * deltaX * deltaX * deltaX;
            return new float2(
                deltaX + Points[segment].x,
                termA + termB + termC + termD);
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

        private void CalculateCubicParameters()
        {
            if(ControlPointCount >= 4)
            {
                // Only initialize the data if there is enough to actually be useful in calculating the data
                m_matrix = new Matrix(Points.Count - 1);

                a = new float[Points.Count];
                b = new float[Points.Count];
                c = new float[Points.Count];
                d = new float[Points.Count];
                segmentDistance = new float[Points.Count - 1];

                CalcParameters();
            }
        }

        private void CalcParameters()
        {
            for (int i = 0; i < Points.Count; i++)
                a[i] = Points[i].y;

            for (int i = 0; i < Points.Count - 1; i++)
                segmentDistance[i] = Points[i + 1].x - Points[i].x;

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
                    m_matrix.a[i, 0] = 2f * (segmentDistance[0] + segmentDistance[1]);
                    m_matrix.a[i, 1] = segmentDistance[1];
                }
                else
                {
                    m_matrix.a[i, i - 1] = segmentDistance[i];
                    m_matrix.a[i, i] = 2f * (segmentDistance[i] + segmentDistance[i + 1]);
                    if(i < Points.Count - 3)
                        m_matrix.a[i, i + 1] = segmentDistance[i + 1];
                }

                if((segmentDistance[i] != 0) && (segmentDistance[i + 1] != 0))
                    m_matrix.y[i] = ((a[i + 2] - a[i + 1]) / segmentDistance[i + 1] - (a[i + 1] - a[i]) / segmentDistance[i]) * 3f;
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
                if(segmentDistance[i] != 0.0)
                {
                    d[i] = 1f / 3f / segmentDistance[i] * (c[i + 1] - c[i]);
                    b[i] = 1f / segmentDistance[i] * (a[i + 1] - a[i]) - segmentDistance[i] / 3f * (c[i + 1] + 2f * c[i]);
                }
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            const float pointDensity = 13;

            if(!DataInitialized)
            {
                // needs to calculate length as it might not have been saved correctly after saving
                RecalculateLengthBias();
            }

            if(ControlPointCount == 2)
            {
                float2 cp0 = GetControlPoint2DWorld(0);
                float2 cp1 = GetControlPoint2DWorld(1);
                Gizmos.DrawLine(new Vector3(cp0.x, cp0.y, 0f), new Vector3(cp1.x, cp1.y, 0f));
                return;
            }

            if(ControlPointCount == 3)
            {
                float2 f = GetControlPoint2DWorld(0);
                Vector3 lp = new Vector3(f.x, f.y, 0f);
                int points = (int) (pointDensity * (SegmentLength[0] * Length()));
                for (int s = 1; s <= points; s++)
                {
                    float progress = s / (float) points;
                    float2 p = ConvertToWorldSpace(Cubic3Point(0, 1, 2, progress));
                    Vector3 point = new Vector3(p.x, p.y, 0f);

                    Gizmos.DrawLine(lp, point);
                    lp = point;
                }
                
                return;
            }

            base.OnDrawGizmosSelected();
        }
    }
}