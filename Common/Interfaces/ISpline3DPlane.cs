using Unity.Mathematics;
using UnityEngine;

namespace Crener.Spline.Common.Interfaces
{
    /// <summary>
    /// Shared functionality for 3D splines
    /// </summary>
    public interface ISpline3DPlane : ISpline3D, ISpline2D
    {
        /// <summary>
        /// Forward direction of the plain
        /// </summary>
        Quaternion Forward { get; set; }
    }

    public interface ISpline3DPlaneEditor : ISpline3DPlane, ISpline3DEditor, ISpline2DEditor {}
}