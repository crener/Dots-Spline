using Crener.Spline.Linear;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._3D
{
    [CustomEditor(typeof(Linear3DSpline))]
    public class Linear3DSplineEditor : Base3DEditor
    {
        private Linear3DSpline pointSpline = null;

        public override void OnInspectorGUI()
        {
            if(target != pointSpline)
            {
                pointSpline = (Linear3DSpline) target;
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
    }
}