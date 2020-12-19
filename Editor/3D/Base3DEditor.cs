using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._3D
{
    public abstract class Base3DEditor : BasicSplineEditorFunctionality
    {
        // editor settings
        protected bool m_editing = false;
        protected bool m_editMoveWithTrans = true;

        // viewing settings (not editing)
        protected int m_debugPointQty = 0;

        // Editing states
        private int m_editNewPointIndex = -1;
        protected int? m_editControlPoint = null;

        protected virtual bool m_inspector3dPoint => true;

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
                    if(m_inspector3dPoint)
                    {
                        float3 currentPoint = spline.GetControlPoint3DLocal(m_editControlPoint.Value);
                        currentPoint = EditorGUILayout.Vector3Field("Point", currentPoint);
                        if(EditorGUI.EndChangeCheck())
                        {
                            spline.UpdateControlPointLocal(m_editControlPoint.Value, currentPoint, SplinePoint.Point);
                            SceneView.RepaintAll();
                        }
                    }
                    else
                    { // useful for spline plane
                        ISpline2DEditor spline2D = spline as ISpline2DEditor;
                        
                        float2 currentPoint = spline2D.GetControlPoint2DLocal(m_editControlPoint.Value);
                        currentPoint = EditorGUILayout.Vector2Field("Point", currentPoint);
                        if(EditorGUI.EndChangeCheck())
                        {
                            spline2D.UpdateControlPointLocal(m_editControlPoint.Value, currentPoint, SplinePoint.Point);
                            SceneView.RepaintAll();
                        }
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

        protected virtual void MoveWithTransform(ISpline3DEditor spline)
        {
            if(m_editMoveWithTrans) return;

            Vector3 currentPosition = m_sourceTrans.position;
            if(currentPosition != m_lastTransPosition)
            {
                Vector3 delta = m_lastTransPosition - currentPosition;
                m_lastTransPosition = currentPosition;

                // move all the points by delta amount
                spline.MoveControlPoints(new float3(delta.x, delta.y, delta.z));
            }
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
                   splinePoint.Equals(spline.GetControlPoint3DLocal(m_editNewPointIndex)))
                    spline.AddControlPoint(splineMousePoint);
                else
                    spline.InsertControlPointWorldSpace(m_editNewPointIndex, splineMousePoint);

                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Renders a blue dot at each control point of the spline and a control handle for the point that is selected
        /// </summary>
        /// <param name="spline">spline to render control points for</param>
        protected void RenderControlPoints(ISpline3DEditor spline)
        {
            for (int i = 0; i < spline.ControlPointCount; i++)
            {
                bool selected = m_editControlPoint == i;
                Handles.color = selected ? Color.blue : new Color(0f, 1f, 1f, 0.73f);

                // draw handles
                if(selected)
                {
                    DrawSelectedHandles(spline, i);
                }
                else
                {
                    Vector3 editorPosition = spline.GetControlPoint3DWorld(i);

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
        /// Draw the control handle for manipulating <paramref name="pointIndex"/> of <paramref name="spline"/>
        /// </summary>
        /// <param name="spline">spline to draw for</param>
        /// <param name="pointIndex">control point index to render from the <paramref name="spline"/></param>
        protected virtual void DrawSelectedHandles(ISpline3DEditor spline, int pointIndex)
        {
            float3 point = spline.GetControlPoint3DWorld(pointIndex);

            EditorGUI.BeginChangeCheck();

            Quaternion handleRotation = Quaternion.identity;
            if(Tools.pivotRotation == PivotRotation.Local && SceneView.lastActiveSceneView != null)
                // point handle away from camera
                handleRotation = LastSceneCameraTrans.rotation;

            float3 pos = Handles.DoPositionHandle(point, handleRotation);

            if(EditorGUI.EndChangeCheck() && spline is Object objSpline)
            {
                Undo.RecordObject(objSpline, "Move Point");
                EditorUtility.SetDirty(objSpline);

                spline.UpdateControlPointWorld(pointIndex, pos, SplinePoint.Point);

                SceneView.RepaintAll();
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
                splinePoint = spline.GetControlPoint3DLocal(0);
            else
            {
                splinePoint = float3.zero;
                float bestDistance = float.MaxValue;

                const int segmentSampleAmount = 64;
                const float segmentSampleAmountF = segmentSampleAmount;
                
                // this could potentially be cached as long as the spline doesn't change and the camera is at the same position
                for (int i = 1; i < spline.SegmentPointCount; i++)
                {
                    for (int s = 0; s <= segmentSampleAmount; s++)
                    {
                        float progress = s / segmentSampleAmountF;
                        float3 p = spline.Get3DPoint(progress, i - 1);
                        Vector3 screenPosition = LastSceneCamera.WorldToScreenPoint(p);

                        float dist = math.distance(mouseComparison, new float2(screenPosition.x, screenPosition.y));
                        if(bestDistance > dist)
                        {
                            splinePoint = p;
                            index = i;
                            bestDistance = dist;
                        }
                    }
                }

                if(!(spline is ILoopingSpline) || !((ILoopingSpline) spline).Looped)
                {
                    if(math.distance(splinePoint, spline.GetControlPoint3DWorld(0)) <= 0.00001f)
                    {
                        index = 0;
                    }
                    else if(math.distance(splinePoint, spline.GetControlPoint3DWorld(spline.ControlPointCount - 1)) <= 0.00001f)
                    {
                        index = spline.ControlPointCount;
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
                float progress = i == 0 ? 0f : i / (quantity - 1f);
                HandleDrawCross(spline.Get3DPointWorld(progress), multiplier);
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