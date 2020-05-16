using System.Collections.Generic;
using Unity.Mathematics;

namespace Crener.Spline.Test
{
    public interface ISimpleTestSpline : ISimpleSpline2DEditor
    {
        IReadOnlyList<float2> ControlPoints { get; }
        IReadOnlyList<float> Times { get; }
        IReadOnlyList<SplineEditMode> Modes { get; }

        float2 GetPoint(float progress);
        void AddControlPoint(float2 point);
        void InsertControlPoint(int index, float2 point);
        void RemoveControlPoint(int index);
    }
    
    public interface IVarianceTestSpline : ISimpleSpline2DVarianceEditor
    {
        IReadOnlyList<float2> ControlPoints { get; }
        IReadOnlyList<float> Times { get; }
        IReadOnlyList<SplineEditMode> Modes { get; }
    }
}