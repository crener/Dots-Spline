using System;
using System.Runtime.CompilerServices;
using Code.Spline2.BezierSpline;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Code.Spline2.Editor
{
    /// <summary>
    /// Editor for <see cref="Spline2DSimple"/> which allows for adjusting control points.
    /// </summary>
    [CustomEditor(typeof(Spline2DSimple))]
    public class Spline2DSimpleEditor : UnityEditor.Editor
    {
        private Spline2DSimple m_spline = null;
        private bool m_editing = false;
        private int? m_editControlPoint = null;
        private bool m_editMoveRelated = true;

        public override void OnInspectorGUI()
        {
            if (target != m_spline)
            {
                m_spline = target as Spline2DSimple;
                m_editing = false;
                m_editControlPoint = null;
            }

            if (m_spline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by {nameof(Spline2DSimpleEditor)}");
                return;
            }

            if (GUILayout.Button("Edit Points"))
            {
                m_editing = !m_editing;
                m_editControlPoint = null;
                SceneView.RepaintAll();
            }

            if (!m_editing)
            {
                GUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    $"Points: {m_spline.ControlPointCount}\n" +
                    $"Length: {m_spline.Length().ToString("N3")}", MessageType.None);
            }
            else
            {
                m_editMoveRelated = EditorGUILayout.Toggle("Move Related Points", m_editMoveRelated);

                if (m_editControlPoint == null)
                {
                    EditorGUILayout.HelpBox($"No Control Point Selected!", MessageType.None);
                }
                else
                {
                    EditorGUILayout.LabelField($"Control Point: {m_editControlPoint}");

                    SplineEditMode existingValue = m_spline.GetEditMode(m_editControlPoint.Value);
                    SplineEditMode editMode = (SplineEditMode) EditorGUILayout.EnumPopup("Edit Mode", existingValue);
                    if (editMode != existingValue)
                    {
                        Undo.RecordObject(m_spline, $"Change EditMode of Point {m_editControlPoint}");
                        EditorUtility.SetDirty(m_spline);
                        m_spline.ChangeEditMode(m_editControlPoint.Value, editMode);

                        SceneView.RepaintAll();
                    }

                    GUILayout.Space(10);
                    if (GUILayout.Button("Remove Point") ||
                       Event.current.isKey && Event.current.type == EventType.KeyDown &&
                       Event.current.keyCode == KeyCode.Delete)
                    {
                        m_spline.RemoveControlPoint(m_editControlPoint.Value);
                        m_editControlPoint = null;

                        SceneView.RepaintAll();

                        if (Event.current.keyCode == KeyCode.Delete)
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
            if (!m_editing) return;

            if (m_editControlPoint.HasValue && AddPointMode())
                m_editControlPoint = null;

            RenderControlPoints();

            if (AddPointMode())
            {
                PointSelection();

                if (Event.current.type == EventType.MouseMove)
                    SceneView.RepaintAll();
            }
        }

        private void PointSelection()
        {
            if (m_spline.ControlPointCount < 2)
            {
                if (LeftClick())
                {
                    Vector3 pos = Input.mousePosition;
                    m_spline.AddControlPoint(new float2(pos.x, pos.y));
                }

                return;
            }

            float2 mouse = MousePos();
            int segmentIndex;
            float2 createPoint = ClosestPointSelection(mouse, out segmentIndex);

            Handles.color = Color.red;
            Handles.DrawLine(
                new Vector3(createPoint.x, createPoint.y, 0f),
                new Vector3(mouse.x, mouse.y, 0f));

            if (LeftClick())
            {
                Undo.RecordObject(m_spline, "Insert Spline Point");
                EditorUtility.SetDirty(m_spline);

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
                m_spline.InsertControlPoint(segmentIndex, mouse);

                SceneView.RepaintAll();
            }
        }

        private float2 ClosestPointSelection(float2 mouse, out int index)
        {
            index = 0;

            float2 bestPoint = float2.zero;
            float bestDistance = float.MaxValue;

            for (int i = 1; i < m_spline.ControlPointCount; i++)
            {
                for (int s = 0; s <= 64; s++)
                {
                    float progress = s / 64f;
                    float2 p = m_spline.GetPoint(progress, i - 1);

                    float dist = math.distance(mouse, p);
                    if (bestDistance > dist)
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
            for (int i = 0; i < m_spline.ControlPointCount; i++)
            {
                bool selected = m_editControlPoint == i;

                if (selected) Handles.color = Color.blue;
                else Handles.color = new Color(0f, 1f, 1f, 0.73f);

                float2 point = m_spline.GetControlPoint(i, SplinePoint.Point);
                Vector3 editorPosition = new Vector3(point.x, point.y);
                SplineEditMode editMode = m_spline.GetEditMode(i);

                // draw handles
                if (selected)
                {
                    Vector3? lastPos = null;
                    bool preExists = i > 0;
                    bool postExists = i != m_spline.ControlPointCount - 1;

                    if (preExists)
                    {
                        // pre Point 
                        float2 pre = m_spline.GetControlPoint(i, SplinePoint.Pre);
                        Vector3 pos = new Vector3(pre.x, pre.y, 0f);
                        lastPos = pos;

                        EditorGUI.BeginChangeCheck();
                        pos = Handles.DoPositionHandle(pos, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(m_spline, "Move Pre Point");
                            EditorUtility.SetDirty(m_spline);

                            float2 newPos = new float2(pos.x, pos.y);

                            switch (editMode)
                            {
                                case SplineEditMode.Standard:
                                {
                                    if (postExists)
                                    {
                                        float postMagnitude = math.length(point - m_spline.GetControlPoint(i, SplinePoint.Post));
                                        UpdateOppositePoint(postMagnitude, point, newPos, i, SplinePoint.Post);
                                    }

                                    m_spline.UpdateControlPoint(i, newPos, SplinePoint.Pre);
                                    break;
                                }
                                case SplineEditMode.Mirror:
                                {
                                    if (postExists)
                                    {
                                        float preMagnitude = math.length(point - newPos);
                                        UpdateOppositePoint(preMagnitude, point, newPos, i, SplinePoint.Post);
                                    }

                                    m_spline.UpdateControlPoint(i, newPos, SplinePoint.Pre);
                                    break;
                                }
                                case SplineEditMode.Free:
                                m_spline.UpdateControlPoint(i, newPos, SplinePoint.Pre);
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

                        if (lastPos != null) Handles.DrawLine(pos, lastPos.Value);
                        lastPos = pos;

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(m_spline, "Move Point");
                            EditorUtility.SetDirty(m_spline);

                            float2 newPoint = new float2(pos.x, pos.y);
                            m_spline.UpdateControlPoint(i, newPoint, SplinePoint.Point);

                            if (m_editMoveRelated)
                            {
                                //move the other points relative to this
                                float2 delta = newPoint - point;
                                if (preExists)
                                    m_spline.UpdateControlPoint(i, m_spline.GetControlPoint(i, SplinePoint.Pre) + delta, SplinePoint.Pre);
                                if (postExists)
                                    m_spline.UpdateControlPoint(i, m_spline.GetControlPoint(i, SplinePoint.Post) + delta, SplinePoint.Post);
                            }

                            SceneView.RepaintAll();
                        }
                    }

                    if (postExists)
                    {
                        // post Point 
                        float2 post = m_spline.GetControlPoint(i, SplinePoint.Post);
                        Vector3 pos = new Vector3(post.x, post.y, 0f);

                        Handles.DrawLine(pos, lastPos.Value);

                        EditorGUI.BeginChangeCheck();
                        pos = Handles.DoPositionHandle(pos, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(m_spline, "Move Post Point");
                            EditorUtility.SetDirty(m_spline);

                            float2 newPos = new float2(pos.x, pos.y);

                            switch (editMode)
                            {
                                case SplineEditMode.Standard:
                                {
                                    if (preExists)
                                    {
                                        float preMagnitude = math.length(point - m_spline.GetControlPoint(i, SplinePoint.Pre));
                                        UpdateOppositePoint(preMagnitude, point, newPos, i, SplinePoint.Pre);
                                    }

                                    m_spline.UpdateControlPoint(i, newPos, SplinePoint.Post);
                                    break;
                                }
                                case SplineEditMode.Mirror:
                                {
                                    if (preExists)
                                    {
                                        float postMagnitude = math.length(point - newPos);
                                        UpdateOppositePoint(postMagnitude, point, newPos, i, SplinePoint.Pre);
                                    }

                                    m_spline.UpdateControlPoint(i, newPos, SplinePoint.Post);
                                    break;
                                }
                                case SplineEditMode.Free:
                                m_spline.UpdateControlPoint(i, newPos, SplinePoint.Post);
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
                    if (Handles.Button(editorPosition, Quaternion.identity, size, size, Handles.DotHandleCap))
                    {
                        m_editControlPoint = i;
                        Repaint();
                    }
                }
            }
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
            m_spline.UpdateControlPoint(i, updatedPoint, pointType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AddPointMode()
        {
            // this is abstracted so that it can be easily ported to the new input system if needed
            return Event.current.shift;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool LeftClick()
        {
            // this is abstracted so that it can be easily ported to the new input system if needed

            Event e = Event.current;
            if (e.type != EventType.MouseDown) return false;
            return e.button == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float2 MousePos()
        {
            // this is abstracted so that it can be easily ported to the new input system if needed
            Vector3 handleResult = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            return new float2(handleResult.x, handleResult.y);
        }
    }
}