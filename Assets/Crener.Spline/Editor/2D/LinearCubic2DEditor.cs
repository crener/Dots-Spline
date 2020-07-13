using System;
using Crener.Spline.BezierSpline;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Linear;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._2D
{
    /// <summary>
    /// Editor for <see cref="BezierSpline2DSimple"/> which allows for adjusting control points.
    /// </summary>
    [CustomEditor(typeof(LinearCubicSpline2D))]
    public class LinearCubic2DEditor : Base2DEditor
    {
        private LinearCubicSpline2D cubicSpline = null;

        public override void OnInspectorGUI()
        {
            if(target != cubicSpline)
            {
                cubicSpline = (LinearCubicSpline2D) target;
                ChangeTransform(cubicSpline.transform);
                m_editing = false;
                m_editControlPoint = null;
            }

            if(cubicSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(LinearCubic2DEditor)}'");
                return;
            }

            OnInspectorGUI(cubicSpline);
        }

        private void OnSceneGUI()
        {
            if(!m_editing)
            {
                if(m_debugPointQty > 0)
                {
                    RenderIntermediateSplinePoints(m_debugPointQty, cubicSpline);
                }

                return;
            }

            if(m_editControlPoint.HasValue && EditorInputAbstractions.AddPointMode())
                m_editControlPoint = null;

            RenderControlPoints(cubicSpline);

            if(EditorInputAbstractions.AddPointMode())
            {
                PointSelection(cubicSpline);

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
            index = 0;

            float2 bestPoint = float2.zero;
            float bestDistance = float.MaxValue;
            //float progress2 = 0f;

            for (int i = 1; i < spline.SegmentPointCount; i++)
            {
                for (int s = 0; s <= 64; s++)
                {
                    float progress = s / 64f;
                    float2 p = spline.GetPoint(progress, i - 1);
                    
                    float dist = math.distance(mouse, p);
                    if(bestDistance > dist)
                    {
                        bestPoint = p;
                        bestDistance = dist;

                        if(progress > 0.5)
                        {
                            index = (i + 1) % spline.ControlPointCount;
                        }
                        else index = i;
                    }
                }
            }

            //Debug.Log($"Failed to find closest point! (best progress: {progress2})");
            return bestPoint;
        }
    }
}