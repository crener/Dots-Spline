using System.Collections.Generic;
using Crener.Spline.BezierSpline;
using Crener.Spline.Common;
using Unity.Mathematics;

namespace Crener.Spline.Test.Variance.TestTypes
{
    /// <summary>
    /// Unity won't allow the creation of a component if it's inside the editor folder so this wraps the type to allow tests to run 
    /// </summary>
    public class MeaninglessTestWrapper2
    {
        /// <summary>
        /// override of <see cref="BezierSpline2DVariance"/> which implements the spline test interface
        /// </summary>
        public class TestBezierSpline2DVarianceJob : BezierSpline2DVariance, IVarianceTestSpline
        {
            public IReadOnlyList<float2> ControlPoints => Points;
            public IReadOnlyList<float> Times
            {
                get
                {
                    if(SegmentLength.Length == 0) return new List<float>();

                    List<float> data = new List<float>(SegmentLength.GetLength(1));
                    for (int i = 0; i < SegmentLength.GetLength(1); i++)
                        data.Add(SegmentLength[0, i]);

                    return data;
                }
            }
            public IReadOnlyList<SplineEditMode> Modes => PointMode;
            public int ExpectedControlPointCount(int controlPoints)
            {
                return math.max(0, ((controlPoints - 1) * FloatsPerControlPoint) + 3);
            }
            
            public int ExpectedTimeCount(int controlPoints) => math.max(1, controlPoints - 1);

            public new void ClearData()
            {
                base.ClearData();
                
                Points.Clear();
                SegmentLength = new float[0, 0];
                PointMode.Clear();

                RecalculateLengthBias();
            }

            public void UpdateControlPoint(int index, float2 point, SplinePoint mode)
            {
                UpdateControlPoint(index, point, TranslateVariancePoint(mode));
            }

            public float2 GetControlPoint(int i, SplinePoint point)
            {
                return GetControlPoint(i, TranslateVariancePoint(point));
            }

            private SplinePointVariance TranslateVariancePoint(SplinePoint point)
            {
                return (SplinePointVariance) point;
            }
        }
    }
}