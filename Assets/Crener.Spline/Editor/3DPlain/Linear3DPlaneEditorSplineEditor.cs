using Crener.Spline.Linear;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._3DPlain
{
    [CustomEditor(typeof(Linear3DPlaneSpline))]
    public class Linear3DPlaneEditorSplineEditor : Base3DPlaneEditor
    {
        private Linear3DPlaneSpline pointSpline = null;

        public override void OnInspectorGUI()
        {
            if(target != pointSpline)
            {
                pointSpline = (Linear3DPlaneSpline) target;
                ChangeTransform(pointSpline.transform);
                m_editing = false;
                m_editControlPoint = null;
            }

            if(pointSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(Linear3DPlaneEditorSplineEditor)}'");
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