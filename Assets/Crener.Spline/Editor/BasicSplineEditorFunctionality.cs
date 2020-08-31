using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor
{
    public class BasicSplineEditorFunctionality : UnityEditor.Editor
    {
        protected Transform m_sourceTrans = null;
        protected Vector3 m_lastTransPosition = Vector3.zero;
        
        protected Camera LastSceneCamera { get; private set; } = null;
        protected Transform LastSceneCameraTrans { get; private set; } = null;

        protected void CheckActiveCamera()
        {
            LastSceneCamera = SceneView.lastActiveSceneView.camera;
            LastSceneCameraTrans = LastSceneCamera.transform;
        }

        protected void ChangeTransform(Transform trans)
        {
            m_sourceTrans = trans;
            m_lastTransPosition = trans.position;
        }
    }
}