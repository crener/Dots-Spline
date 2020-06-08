using System;
using Crener.Spline.BezierSpline;
using Crener.Spline.Common;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._2D
{
    /// <summary>
    /// Editor for <see cref="BezierSpline2DSimple"/> which allows for adjusting control points.
    /// </summary>
    [CustomEditor(typeof(BezierSpline2DSimple))]
    public class BezierSpline2DSimpleEditor : UnityEditor.Editor
    {
        private BezierSpline2DSimple bezierSpline = null;

        private bool m_editing = false;
        private int? m_editControlPoint = null;
        private bool m_editMoveRelated = true;

        private int m_debugPointQty = 0;

        public override void OnInspectorGUI()
        {
            if(target != bezierSpline)
            {
                bezierSpline = target as BezierSpline2DSimple;
                m_editing = false;
                m_editControlPoint = null;
            }

            if(bezierSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(BezierSpline2DSimpleEditor)}'");
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
                    $"Points: {bezierSpline.ControlPointCount}\n" +
                    $"Length: {bezierSpline.Length().ToString("N3")}", MessageType.None);

                EditorGUI.BeginChangeCheck();
                bezierSpline.ArkParameterization = EditorGUILayout.Toggle("Ark Parametrization", bezierSpline.ArkParameterization);
                if(EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(bezierSpline);
                }

                if(bezierSpline.ArkParameterization)
                {
                    EditorGUI.BeginChangeCheck();
                    bezierSpline.ArkLength = EditorGUILayout.FloatField("Ark Length", bezierSpline.ArkLength);
                    if(EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(bezierSpline);
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

                    SplineEditMode existingValue = bezierSpline.GetEditMode(m_editControlPoint.Value);
                    SplineEditMode editMode = (SplineEditMode) EditorGUILayout.EnumPopup("Edit Mode", existingValue);
                    if(editMode != existingValue)
                    {
                        Undo.RecordObject(bezierSpline, $"Change EditMode of Point {m_editControlPoint}");
                        EditorUtility.SetDirty(bezierSpline);
                        bezierSpline.ChangeEditMode(m_editControlPoint.Value, editMode);

                        SceneView.RepaintAll();
                    }

                    GUILayout.Space(10);
                    if(GUILayout.Button("Remove Point") ||
                       Event.current.isKey && Event.current.type == EventType.KeyDown &&
                       Event.current.keyCode == KeyCode.Delete)
                    {
                        bezierSpline.RemoveControlPoint(m_editControlPoint.Value);
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
            if(bezierSpline.ControlPointCount < 2)
            {
                if(InputAbstractions.LeftClick())
                {
                    Vector3 pos = Input.mousePosition;
                    bezierSpline.AddControlPoint(new float2(pos.x, pos.y));
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
                Undo.RecordObject(bezierSpline, "Insert Spline Point");
                EditorUtility.SetDirty(bezierSpline);

                /*if (m_spline.IsLooped)
                {
                    //check if first or last point would result in a better insertion
                    float2 firstPoint = m_spline.GetControlPoint(0, SplinePoint.Point);
                    float2 lastPoint = m_spline.GetControlPoint(m_spline.ControlPointCount - 1, SplinePoint.Point);

                    if (lastPoint.Equals(createPoint)) m_spline.AddControlPoint(mouse);
                    if (firstPoint.Equals(createPoint)) m_spline.InsertControlPoint(0, mouse);
                    else m_spline.InsertControlPoint(segmentIndex, mouse);
                }
                else*/
                bezierSpline.InsertControlPoint(segmentIndex, mouse);

                SceneView.RepaintAll();
            }
        }

        private float2 ClosestPointSelection(float2 mouse, out int index)
        {
            index = 0;

            float2 bestPoint = float2.zero;
            float bestDistance = float.MaxValue;

            for (int i = 1; i < bezierSpline.ControlPointCount; i++)
            {
                for (int s = 0; s <= 64; s++)
                {
                    float progress = s / 64f;
                    float2 p = bezierSpline.GetPoint(progress, i - 1);

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
            for (int i = 0; i < bezierSpline.ControlPointCount; i++)
            {
                bool selected = m_editControlPoint == i;

                if(selected) Handles.color = Color.blue;
                else Handles.color = new Color(0f, 1f, 1f, 0.73f);

                float2 point = bezierSpline.GetControlPoint(i, SplinePoint.Point);
                Vector3 editorPosition = new Vector3(point.x, point.y);
                SplineEditMode editMode = bezierSpline.GetEditMode(i);

                // draw handles
                if(selected)
                {
                    Vector3? lastPos = null;
                    bool preExists = i > 0;
                    bool postExists = i != bezierSpline.ControlPointCount - 1;

                    if(preExists)
                    {
                        // pre Point 
                        float2 pre = bezierSpline.GetControlPoint(i, SplinePoint.Pre);
                        Vector3 pos = new Vector3(pre.x, pre.y, 0f);
                        lastPos = pos;

                        EditorGUI.BeginChangeCheck();
                        pos = Handles.DoPositionHandle(pos, Quaternion.identity);
                        if(EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(bezierSpline, "Move Pre Point");
                            EditorUtility.SetDirty(bezierSpline);

                            float2 newPos = new float2(pos.x, pos.y);

                            switch (editMode)
                            {
                                case SplineEditMode.Standard:
                                {
                                    if(postExists)
                                    {
                                        float postMagnitude = math.length(point - bezierSpline.GetControlPoint(i, SplinePoint.Post));
                                        UpdateOppositePoint(postMagnitude, point, newPos, i, SplinePoint.Post);
                                    }

                                    bezierSpline.UpdateControlPoint(i, newPos, SplinePoint.Pre);
                                    break;
                                }
                                case SplineEditMode.Mirror:
                                {
                                    if(postExists)
                                    {
                                        float preMagnitude = math.length(point - newPos);
                                        UpdateOppositePoint(preMagnitude, point, newPos, i, SplinePoint.Post);
                                    }

                                    bezierSpline.UpdateControlPoint(i, newPos, SplinePoint.Pre);
                                    break;
                                }
                                case SplineEditMode.Free:
                                    bezierSpline.UpdateControlPoint(i, newPos, SplinePoint.Pre);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            SceneView.RepaintAll();
                        }
                    }

                    {
                        // main point 
                        EditorGUI.BeginChangeCheck();
                        Vector3 pos = Handles.DoPositionHandle(editorPosition, Quaternion.identity);

                        if(lastPos != null) Handles.DrawLine(pos, lastPos.Value);
                        lastPos = pos;

                        if(EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(bezierSpline, "Move Point");
                            EditorUtility.SetDirty(bezierSpline);

                            float2 newPoint = new float2(pos.x, pos.y);
                            bezierSpline.UpdateControlPoint(i, newPoint, SplinePoint.Point);

                            if(m_editMoveRelated)
                            {
                                //move the other points relative to this
                                float2 delta = newPoint - point;
                                if(preExists)
                                    bezierSpline.UpdateControlPoint(i, bezierSpline.GetControlPoint(i, SplinePoint.Pre) + delta, SplinePoint.Pre);
                                if(postExists)
                                    bezierSpline.UpdateControlPoint(i, bezierSpline.GetControlPoint(i, SplinePoint.Post) + delta, SplinePoint.Post);
                            }

                            SceneView.RepaintAll();
                        }
                    }

                    if(postExists)
                    {
                        // post Point 
                        float2 post = bezierSpline.GetControlPoint(i, SplinePoint.Post);
                        Vector3 pos = new Vector3(post.x, post.y, 0f);

                        Handles.DrawLine(pos, lastPos.Value);

                        EditorGUI.BeginChangeCheck();
                        pos = Handles.DoPositionHandle(pos, Quaternion.identity);
                        if(EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(bezierSpline, "Move Post Point");
                            EditorUtility.SetDirty(bezierSpline);

                            float2 newPos = new float2(pos.x, pos.y);

                            switch (editMode)
                            {
                                case SplineEditMode.Standard:
                                {
                                    if(preExists)
                                    {
                                        float preMagnitude = math.length(point - bezierSpline.GetControlPoint(i, SplinePoint.Pre));
                                        UpdateOppositePoint(preMagnitude, point, newPos, i, SplinePoint.Pre);
                                    }

                                    bezierSpline.UpdateControlPoint(i, newPos, SplinePoint.Post);
                                    break;
                                }
                                case SplineEditMode.Mirror:
                                {
                                    if(preExists)
                                    {
                                        float postMagnitude = math.length(point - newPos);
                                        UpdateOppositePoint(postMagnitude, point, newPos, i, SplinePoint.Pre);
                                    }

                                    bezierSpline.UpdateControlPoint(i, newPos, SplinePoint.Post);
                                    break;
                                }
                                case SplineEditMode.Free:
                                    bezierSpline.UpdateControlPoint(i, newPos, SplinePoint.Post);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            SceneView.RepaintAll();
                        }
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
                HandleDrawCross(bezierSpline.GetPoint(i == 0f ? 0f : i / (quantity - 1)), multiplier);
            }
        }

        private void HandleDrawCross(Vector2 location, float sizeMultiplier = 1f)
        {
            Vector3 worldLocation = new Vector3(location.x, location.y, 0f);
            float handleSize = HandleUtility.GetHandleSize(worldLocation) * sizeMultiplier;

            Handles.DrawLine(worldLocation + (Vector3.right * handleSize), worldLocation - (Vector3.right * handleSize));
            Handles.DrawLine(worldLocation + (Vector3.up * handleSize), worldLocation - (Vector3.up * handleSize));
        }

        /// <summary>
        /// Updates point based on given <paramref name="magnitude"/>
        /// </summary>
        /// <param name="magnitude">distance from the center to the point</param>
        /// <param name="center">center point</param>
        /// <param name="newPos">new point position</param>
        /// <param name="i">spline control point index</param>
        /// <param name="pointType">point type to update with newly computed position</param>
        private void UpdateOppositePoint(float magnitude, float2 center, float2 newPos, int i, SplinePoint pointType)
        {
            float2 delta = center - newPos;
            float angle = math.atan2(delta.y, delta.x) - (math.PI / 2f);

            // match the opposite angle for the pointType
            float2 updatedPoint = new float2(
                center.x + (math.sin(-angle) * magnitude),
                center.y + (math.cos(-angle) * magnitude));
            bezierSpline.UpdateControlPoint(i, updatedPoint, pointType);
        }
    }
}