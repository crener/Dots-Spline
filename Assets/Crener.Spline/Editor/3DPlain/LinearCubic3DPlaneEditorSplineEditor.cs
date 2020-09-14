using Crener.Spline.Linear;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._3DPlain
{
    [CustomEditor(typeof(LinearCubic3DPlaneSpline))]
    public class LinearCubic3DPlaneEditorSplineEditor : Base3DPlaneEditor
    {
        private LinearCubic3DPlaneSpline pointSpline = null;

        public override void OnInspectorGUI()
        {
            if(target != pointSpline)
            {
                pointSpline = (LinearCubic3DPlaneSpline) target;
                ChangeTransform(pointSpline.transform);
                m_editing = false;
                m_editControlPoint = null;
            }

            if(pointSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(LinearCubic3DPlaneEditorSplineEditor)}'");
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