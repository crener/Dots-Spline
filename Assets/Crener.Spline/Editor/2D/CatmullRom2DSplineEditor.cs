using Crener.Spline.BezierSpline;
using Crener.Spline.CatmullRom;
using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Linear;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._2D
{
    /// <summary>
    /// Editor for <see cref="Linear2DSpline"/> which allows for adjusting control points.
    /// </summary>
    [CustomEditor(typeof(CatmullRom2DSpline))]
    public class CatmullRom2DSplineEditor : Base2DEditor
    {
        private CatmullRom2DSpline pointSpline = null;

        public override void OnInspectorGUI()
        {
            if(target != pointSpline)
            {
                pointSpline = (CatmullRom2DSpline) target;
                ChangeTransform(pointSpline.transform);
                m_editing = false;
                m_editControlPoint = null;
            }

            if(pointSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(CatmullRom2DSplineEditor)}'");
                return;
            }
            
            OnInspectorGUI(pointSpline);
        }

        private void OnSceneGUI()
        {
            if(!m_editing)
            {
                if(m_debugPointQty > 0)
                {
                    RenderIntermediateSplinePoints(m_debugPointQty, pointSpline);
                }

                return;
            }

            if(m_editControlPoint.HasValue && EditorInputAbstractions.AddPointMode())
                m_editControlPoint = null;

            RenderControlPoints(pointSpline);

            if(EditorInputAbstractions.AddPointMode())
            {
                PointSelection(pointSpline);

                if(Event.current.type == EventType.MouseMove)
                    SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Find the closest point to the spline
        /// </summary>
        /// <param name="mouse">mouse position</param>
        /// <param name="spline">spline to check</param>
        /// <param name="index">spline index of the closest point</param>
        /// <returns>closest point on the spline to the mouse</returns>
        protected override float2 ClosestPointSelection(float2 mouse, ISpline2D spline, out int index)
        {
            Assert.AreSame(pointSpline, spline, "Somehow the spline changed from the start point and the point sample point");

            float2 closest = base.ClosestPointSelection(mouse, spline, out index);
            index = pointSpline.Looped ? (index + 1) % spline.ControlPointCount : index;
            return closest;
        }
    }
}