using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._2D
{
    public abstract class Base2DEditor : UnityEditor.Editor
    {
        protected bool m_editing = false;
        protected int? m_editControlPoint = null;
        protected bool m_editMoveRelated = true;

        protected int m_debugPointQty = 0;

        /// <summary>
        /// Shared Inspector logic
        /// </summary>
        /// <param name="spline">spline to show inspector for</param>
        protected void OnInspectorGUI(ISpline2DEditor spline)
        {
            if(GUILayout.Button("Edit Points"))
            {
                m_editing = !m_editing;
                m_editControlPoint = null;
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
                m_editMoveRelated = EditorGUILayout.Toggle("Move Related Points", m_editMoveRelated);

                if(m_editControlPoint == null)
                {
                    EditorGUILayout.HelpBox($"No Control Point Selected!", MessageType.None);
                }
                else
                {
                    EditorGUILayout.LabelField($"Control Point: {m_editControlPoint}");

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

        /// <summary>
        /// Handles the control logic for adding a point. This will draw a red line from the mouse position to the closest point along the
        /// curve
        /// </summary>
        /// <param name="spline">spline to handle control point logic for</param>
        protected void PointSelection(ISpline2DEditor spline)
        {
            if(spline.ControlPointCount < 2)
            {
                if(InputAbstractions.LeftClick())
                {
                    Vector3 pos = Input.mousePosition;
                    spline.AddControlPoint(new float2(pos.x, pos.y));
                }

                return;
            }

            float2 mouse = InputAbstractions.MousePos();
            int segmentIndex;
            float2 createPoint = ClosestPointSelection(mouse, spline, out segmentIndex);

            Handles.color = Color.red;
            Handles.DrawLine(
                new Vector3(createPoint.x, createPoint.y, 0f),
                new Vector3(mouse.x, mouse.y, 0f));

            if(InputAbstractions.LeftClick() && spline is Object objSpline)
            {
                Undo.RecordObject(objSpline, "Insert Spline Point");
                EditorUtility.SetDirty(objSpline);

                spline.InsertControlPoint(segmentIndex, mouse);

                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// Renders a blue dot at each control point of the spline
        /// </summary>
        /// <param name="spline">spline to render control points for</param>
        protected void RenderControlPoints(ISpline2DEditor spline)
        {
            for (int i = 0; i < spline.ControlPointCount; i++)
            {
                bool selected = m_editControlPoint == i;

                Handles.color = selected ? Color.blue : new Color(0f, 1f, 1f, 0.73f);

                float2 point = spline.GetControlPoint(i);
                Vector3 editorPosition = new Vector3(point.x, point.y);

                // draw handles
                if(selected)
                {
                    // main point 
                    EditorGUI.BeginChangeCheck();
                    Vector3 pos = Handles.DoPositionHandle(editorPosition, Quaternion.identity);

                    if(EditorGUI.EndChangeCheck() && spline is Object objSpline)
                    {
                        Undo.RecordObject(objSpline, "Move Point");
                        EditorUtility.SetDirty(objSpline);

                        float2 newPoint = new float2(pos.x, pos.y);
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
        /// <returns>closest point on the spline to the mouse</returns>
        protected static float2 ClosestPointSelection(float2 mouse, ISpline2D spline, out int index)
        {
            index = 0;

            float2 bestPoint = float2.zero;
            float bestDistance = float.MaxValue;

            for (int i = 1; i < spline.ControlPointCount; i++)
            {
                for (int s = 0; s <= 64; s++)
                {
                    float progress = s / 64f;
                    float2 p = spline.GetPoint(progress, i - 1);

                    float dist = math.distance(mouse, p);
                    if(bestDistance > dist)
                    {
                        bestPoint = p;
                        index = i;
                        bestDistance = dist;
                    }
                }
            }

            return bestPoint;
        }

        /// <summary>
        /// Renders points along the spline to demonstrate possible point bunching along the splines length
        /// </summary>
        /// <param name="quantity">amount of points to render</param>
        /// <param name="spline">spline to render on</param>
        protected static void RenderIntermediateSplinePoints(float quantity, ISpline2D spline)
        {
            Handles.color = Color.red;
            const float multiplier = 0.3f;

            for (int i = 0; i <= quantity; i++)
            {
                HandleDrawCross(spline.GetPoint(i == 0 ? 0f : i / (quantity - 1f)), multiplier);
            }
        }

        private static void HandleDrawCross(Vector2 location, float sizeMultiplier = 1f)
        {
            Vector3 worldLocation = new Vector3(location.x, location.y, 0f);
            float handleSize = HandleUtility.GetHandleSize(worldLocation) * sizeMultiplier;

            Handles.DrawLine(worldLocation + (Vector3.right * handleSize), worldLocation - (Vector3.right * handleSize));
            Handles.DrawLine(worldLocation + (Vector3.up * handleSize), worldLocation - (Vector3.up * handleSize));
        }
    }
}