using Crener.Spline._2D;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._2D
{
    /// <summary>
    /// Editor for <see cref="BezierSpline2DSimple"/> which allows for adjusting control points.
    /// </summary>
    [CustomEditor(typeof(LinearCubic2DSpline))]
    public class LinearCubic2DEditor : Base2DEditor
    {
        private LinearCubic2DSpline cubic2DSpline = null;

        public override void OnInspectorGUI()
        {
            if(target != cubic2DSpline)
            {
                cubic2DSpline = (LinearCubic2DSpline) target;
                ChangeTransform(cubic2DSpline.transform);
                m_editing = false;
                m_editControlPoint = null;
            }

            if(cubic2DSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(LinearCubic2DEditor)}'");
                return;
            }

            OnInspectorGUI(cubic2DSpline);
        }

        private void OnSceneGUI()
        {
            if(!m_editing)
            {
                if(m_debugPointQty > 0)
                {
                    RenderIntermediateSplinePoints(m_debugPointQty, cubic2DSpline);
                }

                return;
            }

            if(m_editControlPoint.HasValue && EditorInputAbstractions.AddPointMode())
                m_editControlPoint = null;

            RenderControlPoints(cubic2DSpline);

            if(EditorInputAbstractions.AddPointMode())
            {
                PointSelection(cubic2DSpline);

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
        protected override float2 ClosestPointSelection(float2 mouse, ISpline2DEditor spline, out int index)
        {
            index = 0;

            if(spline.ControlPointCount == 1)
                return spline.GetControlPoint2DLocal(0);
            else if(spline.ControlPointCount == 0)
                return mouse;

            float2 bestPoint = float2.zero;
            float bestDistance = float.MaxValue;

            for (int i = 1; i < spline.SegmentPointCount; i++)
            {
                for (int s = 0; s <= 64; s++)
                {
                    float progress = s / 64f;
                    float2 p = spline.Get2DPointWorld(progress, i - 1);
                    
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

            if(!(spline is ILoopingSpline) || !((ILoopingSpline) spline).Looped)
            {
                if(math.distance(bestPoint, spline.GetControlPoint2DWorld(0)) <= 0.00001f)
                {
                    index = 0;
                }
                else if(math.distance(bestPoint, spline.GetControlPoint2DWorld(spline.ControlPointCount - 1)) <= 0.00001f)
                {
                    index = spline.ControlPointCount - 1;
                }
            }

            return bestPoint;
        }
    }
}