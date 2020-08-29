using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Crener.Spline.Editor._3D
{
    public abstract class Base3DEditor : UnityEditor.Editor
    {
        private Transform m_sourceTrans = null;
        private Vector3 m_lastTransPosition = Vector3.zero;
        protected Camera LastSceneCamera { get; private set; } = null;
        protected Transform LastSceneCameraTrans { get; private set; } = null;

        // editor settings
        protected bool m_editing = false;
        protected bool m_editMoveWithTrans = true;

        // viewing settings (not editing)
        protected int m_debugPointQty = 0;

        // Editing states
        private int m_editNewPointIndex = -1;
        protected int? m_editControlPoint = null;

        #region Inspector
        /// <summary>
        /// Shared Inspector logic
        /// </summary>
        /// <param name="spline">spline to show inspector for</param>
        protected void OnInspectorGUI(ISpline3DEditor spline)
        {
            if(GUILayout.Button("Edit Points"))
            {
                m_editing = !m_editing;
                m_editControlPoint = null;
                m_editNewPointIndex = -1;
                m_lastTransPosition = m_sourceTrans?.position ?? Vector3.zero;

                SceneView.RepaintAll();
            }

            if(!m_editing)
            {
                GUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    $"Points: {spline.ControlPointCount}\n" +
                    $"Length: {spline.Length():N3}", MessageType.None);

                if(spline is ILoopingSpline loop)
                {
                    EditorGUI.BeginChangeCheck();
                    loop.Looped = EditorGUILayout.Toggle("Loop", loop.Looped);
                    if(EditorGUI.EndChangeCheck())
                    {
                        SceneView.RepaintAll();
                    }
                }

                if(spline is IArkableSpline arked)
                {
                    EditorGUI.BeginChangeCheck();
                    arked.ArkParameterization = EditorGUILayout.Toggle("Ark Parametrization", arked.ArkParameterization);
                    if(EditorGUI.EndChangeCheck() && spline is Object objSpline)
                    {
                        EditorUtility.SetDirty(objSpline);
                    }

                    if(arked.ArkParameterization)
                    {
                        EditorGUI.BeginChangeCheck();
                        arked.ArkLength = EditorGUILayout.FloatField("Ark Length", arked.ArkLength);
                        if(EditorGUI.EndChangeCheck() && spline is Object objSpline2)
                        {
                            EditorUtility.SetDirty(objSpline2);
                        }
                    }
                }

                GUILayout.Space(5);

                // Draw spline points - Draw a point every set interval to illustrate where bezier points will be
                EditorGUI.BeginChangeCheck();
                m_debugPointQty = (int) EditorGUILayout.Slider("Draw points", m_debugPointQty, 0, 200);
                if(EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }
            else
            {
                if(m_sourceTrans != null)
                {
                    bool value = EditorGUILayout.Toggle("Move Points with Transform", m_editMoveWithTrans);
                    if(value != m_editMoveWithTrans)
                    {
                        m_editMoveWithTrans = value;
                        m_lastTransPosition = m_sourceTrans.position;
                    }

                    MoveWithTransform(spline);
                }

                if(m_editNewPointIndex >= 0)
                {
                    EditorGUILayout.LabelField($"New At Index: {m_editNewPointIndex}");
                }

                if(m_editControlPoint == null)
                {
                    EditorGUILayout.HelpBox($"No Control Point Selected!", MessageType.None);
                }
                else
                {
                    m_editNewPointIndex = -1;
                    EditorGUILayout.LabelField($"Control Point: {m_editControlPoint}");

                    // show the current control point location
                    EditorGUI.BeginChangeCheck();
                    float3 currentPoint = spline.GetControlPoint(m_editControlPoint.Value);
                    currentPoint = EditorGUILayout.Vector3Field("Point", currentPoint);
                    if(EditorGUI.EndChangeCheck())
                    {
                        spline.UpdateControlPoint(m_editControlPoint.Value, currentPoint, SplinePoint.Point);
                        SceneView.RepaintAll();
                    }

                    SplineEditMode existingValue = spline.GetEditMode(m_editControlPoint.Value);
                    SplineEditMode editMode = (SplineEditMode) EditorGUILayout.EnumPopup("Edit Mode", existingValue);
                    if(editMode != existingValue && spline is Object objSpline)
                    {
                        Undo.RecordObject(objSpline, $"Change EditMode of Point {m_editControlPoint}");
                        EditorUtility.SetDirty(objSpline);
                        spline.ChangeEditMode(m_editControlPoint.Value, editMode);

                        SceneView.RepaintAll();
                    }

                    GUILayout.Space(10);
                    if(GUILayout.Button("Remove Point") ||
                       Event.current.isKey && Event.current.type == EventType.KeyDown &&
                       Event.current.keyCode == KeyCode.Delete)
                    {
                        spline.RemoveControlPoint(m_editControlPoint.Value);
                        m_editControlPoint = null;

                        SceneView.RepaintAll();

                        if(Event.current.keyCode == KeyCode.Delete)
                        {
                            // Sadly this doesn't actually work :( dammit Unity
                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                        }
                    }
                }
            }
        }

        private void MoveWithTransform(ISpline3DEditor spline)
        {
            if(!m_editMoveWithTrans) return;

            Vector3 currentPosition = m_sourceTrans.position;
            if(currentPosition != m_lastTransPosition)
            {
                Vector3 delta = currentPosition - m_lastTransPosition;
                m_lastTransPosition = currentPosition;

                // move all the points by delta amount
                spline.MoveControlPoints(new float3(delta.x, delta.y, delta.z));
            }
        }

        protected void ChangeTransform(Transform trans)
        {
            m_sourceTrans = trans;
            m_lastTransPosition = trans.position;
        }
        #endregion Inspector

        /// <summary>
        /// Handles the control logic for adding a point. This will draw a red line from the mouse position to the closest point along the
        /// curve
        /// </summary>
        /// <param name="spline">spline to handle control point logic for</param>
        protected void PointSelection(ISpline3DEditor spline)
        {
            int splineIndex;
            float3 splinePoint, splineMousePoint;
            ClosestPointSelection(Event.current.mousePosition, spline, out splineIndex, out splinePoint, out splineMousePoint);

            if(splineIndex != m_editNewPointIndex)
            {
                m_editNewPointIndex = splineIndex;
                Repaint(); // force refresh inspector
            }

            Handles.color = Color.red;
            Handles.DrawLine(splinePoint, splineMousePoint);

            if(EditorInputAbstractions.LeftClick() && spline is Object objSpline)
            {
                Undo.RecordObject(objSpline, "Insert Spline Point");
                EditorUtility.SetDirty(objSpline);

                if((m_editNewPointIndex == spline.ControlPointCount - 1 || spline.ControlPointCount == 1) &&
                   splinePoint.Equals(spline.GetControlPoint(m_editNewPointIndex)))
                    spline.AddControlPoint(splineMousePoint);
                else
                    spline.InsertControlPoint(m_editNewPointIndex, splineMousePoint);

                SceneView.RepaintAll();
            }
        }

        protected void CheckActiveCamera()
        {
            LastSceneCamera = SceneView.lastActiveSceneView.camera;
            LastSceneCameraTrans = LastSceneCamera.transform;
        }

        /// <summary>
        /// Renders a blue dot at each control point of the spline
        /// </summary>
        /// <param name="spline">spline to render control points for</param>
        protected void RenderControlPoints(ISpline3DEditor spline)
        {
            for (int i = 0; i < spline.ControlPointCount; i++)
            {
                bool selected = m_editControlPoint == i;

                Handles.color = selected ? Color.blue : new Color(0f, 1f, 1f, 0.73f);

                float3 point = spline.GetControlPoint(i);
                Vector3 editorPosition = new Vector3(point.x, point.y, point.z);

                // draw handles
                if(selected)
                {
                    // main point 
                    EditorGUI.BeginChangeCheck();

                    Quaternion handleRotation = Quaternion.identity;
                    if(Tools.pivotRotation == PivotRotation.Local && SceneView.lastActiveSceneView != null)
                        // point handle away from camera
                        handleRotation = LastSceneCameraTrans.rotation;

                    Vector3 pos = Handles.DoPositionHandle(editorPosition, handleRotation);

                    if(EditorGUI.EndChangeCheck() && spline is Object objSpline)
                    {
                        Undo.RecordObject(objSpline, "Move Point");
                        EditorUtility.SetDirty(objSpline);

                        float3 newPoint = new float3(pos.x, pos.y, pos.z);
                        spline.UpdateControlPoint(i, newPoint, SplinePoint.Point);

                        SceneView.RepaintAll();
                    }
                }
                else
                {
                    float size = HandleUtility.GetHandleSize(editorPosition) / 12f;
                    if(Handles.Button(editorPosition, Quaternion.identity, size, size, Handles.DotHandleCap))
                    {
                        m_editControlPoint = i;
                        Repaint();
                    }
                }
            }
        }

        /// <summary>
        /// Find the closest point to the spline
        /// </summary>
        /// <param name="mouse">mouse position</param>
        /// <param name="spline">spline to check</param>
        /// <param name="index">spline index of the closest point</param>
        /// <param name="splinePoint">Point on spline that is closest</param>
        /// <param name="creationPoint">point that aligned to the mouse point relative to the camera</param>
        /// <returns>true is point could be found</returns>
        protected virtual bool ClosestPointSelection(float2 mouse, ISpline3DEditor spline, out int index, out float3 splinePoint,
            out float3 creationPoint)
        {
            index = 0;
            float2 mouseComparison = new float2(mouse.x, LastSceneCamera.pixelHeight - mouse.y);

            if(spline.ControlPointCount == 0)
                splinePoint = EditorInputAbstractions.MousePos3D();
            if(spline.ControlPointCount == 1)
                splinePoint = spline.GetControlPoint(0);
            else
            {
                splinePoint = float3.zero;
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
                            index = i;
                            bestDistance = dist;
                        }
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

        /// <summary>
        /// Renders points along the spline to demonstrate possible point bunching along the splines length
        /// </summary>
        /// <param name="quantity">amount of points to render</param>
        /// <param name="spline">spline to render on</param>
        protected static void RenderIntermediateSplinePoints(float quantity, ISpline3D spline)
        {
            Handles.color = Color.red;
            const float multiplier = 0.3f;

            for (int i = 0; i <= quantity; i++)
            {
                HandleDrawCross(spline.Get3DPoint(i == 0 ? 0f : i / (quantity - 1f)), multiplier);
            }
        }

        protected static void HandleDrawCross(Vector3 location, float sizeMultiplier = 1f)
        {
            Vector3 worldLocation = new Vector3(location.x, location.y, location.z);
            float handleSize = HandleUtility.GetHandleSize(worldLocation) * sizeMultiplier;

            Handles.DrawLine(worldLocation + (Vector3.right * handleSize), worldLocation - (Vector3.right * handleSize));
            Handles.DrawLine(worldLocation + (Vector3.up * handleSize), worldLocation - (Vector3.up * handleSize));
            Handles.DrawLine(worldLocation + (Vector3.forward * handleSize), worldLocation - (Vector3.forward * handleSize));
        }
    }
}