using Crener.Spline._3D;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._3D
{
    [CustomEditor(typeof(LinearCubic3DSpline))]
    public class LinearCubic3DSplineEditor : Base3DEditor
    {
        private LinearCubic3DSpline pointSpline = null;

        public override void OnInspectorGUI()
        {
            if(target != pointSpline)
            {
                pointSpline = (LinearCubic3DSpline) target;
                ChangeTransform(pointSpline.transform);
                m_editing = false;
                m_editControlPoint = null;
            }

            if(pointSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(Linear3DSplineEditor)}'");
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

            CheckActiveCamera();
            RenderControlPoints(pointSpline);

            if(EditorInputAbstractions.AddPointMode() && 
               SceneView.currentDrawingSceneView == SceneView.lastActiveSceneView)
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
        protected override bool ClosestPointSelection(float2 mouse, ISpline3DEditor spline, out int index, out float3 splinePoint,
            out float3 creationPoint)
        {
            index = 0;
            splinePoint = float3.zero;

            float2 mouseComparison = new float2(mouse.x, LastSceneCamera.pixelHeight - mouse.y);
            float bestDistance = float.MaxValue;

            // this could potentially be cached as long as the spline doesn't change and the camera is at the same position
            for (int i = 1; i < spline.SegmentPointCount; i++)
            {
                for (int s = 0; s <= 64; s++)
                {
                    float progress = s / 64f;
                    float3 p = spline.Get3DPoint(progress, i - 1);
                    Vector3 screenPosition = LastSceneCamera.WorldToScreenPoint(p);

                    HandleDrawCross(screenPosition, 0.5f);

                    float dist = math.distance(mouseComparison, new float2(screenPosition.x, screenPosition.y));
                    if(bestDistance > dist)
                    {
                        splinePoint = p;
                        bestDistance = dist;
                        
                        if(progress > 0.5)
                        {
                            index = (i + 1) % spline.ControlPointCount;
                        }
                        else index = i;
                    }
                }
            }

            // convert the spline point and mouse position into the new point position
            if(!splinePoint.Equals(float3.zero))
            {
                Vector3 camPosition = LastSceneCameraTrans.position;

                float worldDistance = math.distance(splinePoint, camPosition);
                Ray cameraRay = LastSceneCamera.ScreenPointToRay(new Vector3(mouseComparison.x, mouseComparison.y, 0f));

                creationPoint = cameraRay.GetPoint(worldDistance);
                return true;
            }

            creationPoint = default;
            return false;
        }
    }
}