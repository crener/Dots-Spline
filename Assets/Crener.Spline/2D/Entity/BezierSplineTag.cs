using Crener.Spline.Common.DataStructs;
using Unity.Entities;

namespace Crener.Spline._2D.Entity
{
    /// <summary>
    /// Tag to allow a system to determine the type of data <see cref="Spline2DData"/> contains
    /// </summary>
    public class BezierSplineTag : IComponentData { }
}