using Crener.Spline.BezierSpline;
using Crener.Spline.Common;
using Crener.Spline.PointToPoint;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._2D
{
    /// <summary>
    /// Editor for <see cref="BezierSpline2DSimple"/> which allows for adjusting control points.
    /// </summary>
    [CustomEditor(typeof(PointToPoint2DSpline))]
    public class PointToPoint2DSplineEditor : UnityEditor.Editor
    {
        private PointToPoint2DSpline pointSpline = null;

        private bool m_editing = false;
        private int? m_editControlPoint = null;
        private bool m_editMoveRelated = true;

        private int m_debugPointQty = 0;

        public override void OnInspectorGUI()
        {
            if(target != pointSpline)
            {
                pointSpline = target as PointToPoint2DSpline;
                m_editing = false;
                m_editControlPoint = null;
            }

            if(pointSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(PointToPoint2DSplineEditor)}'");
                return;
            }

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
                    $"Points: {pointSpline.ControlPointCount}\n" +
                    $"Length: {pointSpline.Length().ToString("N3")}", MessageType.None);

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

                    SplineEditMode existingValue = pointSpline.GetEditMode(m_editControlPoint.Value);
                    SplineEditMode editMode = (SplineEditMode) EditorGUILayout.EnumPopup("Edit Mode", existingValue);
                    if(editMode != existingValue)
                    {
                        Undo.RecordObject(pointSpline, $"Change EditMode of Point {m_editControlPoint}");
                        EditorUtility.SetDirty(pointSpline);
                        pointSpline.ChangeEditMode(m_editControlPoint.Value, editMode);

                        SceneView.RepaintAll();
                    }

                    GUILayout.Space(10);
                    if(GUILayout.Button("Remove Point") ||
                       Event.current.isKey && Event.current.type == EventType.KeyDown &&
                       Event.current.keyCode == KeyCode.Delete)
                    {
                        pointSpline.RemoveControlPoint(m_editControlPoint.Value);
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

        private void OnSceneGUI()
        {
            if(!m_editing)
            {
                if(m_debugPointQty > 0)
                {
                    RenderIntermediateSplinePoints(m_debugPointQty);
                }

                return;
            }

            if(m_editControlPoint.HasValue && InputAbstractions.AddPointMode())
                m_editControlPoint = null;

            RenderControlPoints();

            if(InputAbstractions.AddPointMode())
            {
                PointSelection();

                if(Event.current.type == EventType.MouseMove)
                    SceneView.RepaintAll();
            }
        }

        private void PointSelection()
        {
            if(pointSpline.ControlPointCount < 2)
            {
                if(InputAbstractions.LeftClick())
                {
                    Vector3 pos = Input.mousePosition;
                    pointSpline.AddControlPoint(new float2(pos.x, pos.y));
                }

                return;
            }

            float2 mouse = InputAbstractions.MousePos();
            int segmentIndex;
            float2 createPoint = ClosestPointSelection(mouse, out segmentIndex);

            Handles.color = Color.red;
            Handles.DrawLine(
                new Vector3(createPoint.x, createPoint.y, 0f),
                new Vector3(mouse.x, mouse.y, 0f));

            if(InputAbstractions.LeftClick())
            {
                Undo.RecordObject(pointSpline, "Insert Spline Point");
                EditorUtility.SetDirty(pointSpline);

                pointSpline.InsertControlPoint(segmentIndex, mouse);

                SceneView.RepaintAll();
            }
        }

        private float2 ClosestPointSelection(float2 mouse, out int index)
        {
            index = 0;

            float2 bestPoint = float2.zero;
            float bestDistance = float.MaxValue;

            for (int i = 1; i < pointSpline.ControlPointCount; i++)
            {
                for (int s = 0; s <= 64; s++)
                {
                    float progress = s / 64f;
                    float2 p = pointSpline.GetPoint(progress, i - 1);

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

        private void RenderControlPoints()
        {
            for (int i = 0; i < pointSpline.ControlPointCount; i++)
            {
                bool selected = m_editControlPoint == i;

                if(selected) Handles.color = Color.blue;
                else Handles.color = new Color(0f, 1f, 1f, 0.73f);

                float2 point = pointSpline.GetControlPoint(i);
                Vector3 editorPosition = new Vector3(point.x, point.y);

                // draw handles
                if(selected)
                {
                    // main point 
                    EditorGUI.BeginChangeCheck();
                    Vector3 pos = Handles.DoPositionHandle(editorPosition, Quaternion.identity);

                    if(EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(pointSpline, "Move Point");
                        EditorUtility.SetDirty(pointSpline);

                        float2 newPoint = new float2(pos.x, pos.y);
                        pointSpline.UpdateControlPoint(i, newPoint, SplinePoint.Point);

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

        private void RenderIntermediateSplinePoints(float quantity)
        {
            Handles.color = Color.red;
            const float multiplier = 0.3f;

            for (float i = 0; i <= quantity; i++)
            {
                HandleDrawCross(pointSpline.GetPoint(i == 0f ? 0f : i / (quantity - 1)), multiplier);
            }
        }

        private void HandleDrawCross(Vector2 location, float sizeMultiplier = 1f)
        {
            Vector3 worldLocation = new Vector3(location.x, location.y, 0f);
            float handleSize = HandleUtility.GetHandleSize(worldLocation) * sizeMultiplier;

            Handles.DrawLine(worldLocation + (Vector3.right * handleSize), worldLocation - (Vector3.right * handleSize));
            Handles.DrawLine(worldLocation + (Vector3.up * handleSize), worldLocation - (Vector3.up * handleSize));
        }
    }
}