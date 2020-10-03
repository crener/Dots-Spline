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
    public class BezierSpline2DSimpleEditor : Base2DEditor
    {
        private BezierSpline2DSimple bezierSpline = null;

        public override void OnInspectorGUI()
        {
            if(target != bezierSpline)
            {
                bezierSpline = (BezierSpline2DSimple) target;
                ChangeTransform(bezierSpline.transform);
                m_editing = false;
                m_editControlPoint = null;
            }

            if(bezierSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(BezierSpline2DSimpleEditor)}'");
                return;
            }
            
            OnInspectorGUI(bezierSpline);
        }
        
        private void OnSceneGUI()
        {
            if(!m_editing)
            {
                if(m_debugPointQty > 0)
                {
                    RenderIntermediateSplinePoints(m_debugPointQty, bezierSpline);
                }

                return;
            }

            if(m_editControlPoint.HasValue && EditorInputAbstractions.AddPointMode())
                m_editControlPoint = null;

            RenderControlPoints();

            if(EditorInputAbstractions.AddPointMode())
            {
                PointSelection(bezierSpline);

                if(Event.current.type == EventType.MouseMove)
                    SceneView.RepaintAll();
            }
        }
        
        protected void RenderControlPoints()
        {
            for (int i = 0; i < bezierSpline.ControlPointCount; i++)
            {
                bool selected = m_editControlPoint == i;

                Handles.color = selected ? Color.blue : new Color(0f, 1f, 1f, 0.73f);

                float2 point = bezierSpline.GetControlPoint2DWorld(i, SplinePoint.Point);
                Vector3 editorPosition = new Vector3(point.x, point.y);
                SplineEditMode editMode = bezierSpline.GetEditMode(i);

                // draw handles
                if(selected)
                {
                    float3 origin = m_sourceTrans.position;
                    point -= origin.xy;
                    Vector3? lastPos = null;
                    bool preExists = i > 0;
                    bool postExists = i != bezierSpline.ControlPointCount - 1;

                    if(preExists)
                    {
                        // pre Point 
                        float2 pre = bezierSpline.GetControlPoint2DWorld(i, SplinePoint.Pre);
                        Vector3 pos = new Vector3(pre.x, pre.y, 0f);
                        lastPos = pos;

                        EditorGUI.BeginChangeCheck();
                        pos = Handles.DoPositionHandle(pos, Quaternion.identity);
                        if(EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(bezierSpline, "Move Pre Point");
                            EditorUtility.SetDirty(bezierSpline);

                            float2 newPos = new float2(pos.x, pos.y) - origin.xy;

                            switch (editMode)
                            {
                                case SplineEditMode.Standard:
                                {
                                    if(postExists)
                                    {
                                        float postMagnitude = math.length(point - bezierSpline.GetControlPoint(i, SplinePoint.Post));
                                        UpdateOppositePoint(postMagnitude, point, newPos, i, SplinePoint.Post);
                                    }

                                    bezierSpline.UpdateControlPointLocal(i, newPos, SplinePoint.Pre);
                                    break;
                                }
                                case SplineEditMode.Mirror:
                                {
                                    if(postExists)
                                    {
                                        float preMagnitude = math.length(point - newPos);
                                        UpdateOppositePoint(preMagnitude, point, newPos, i, SplinePoint.Post);
                                    }

                                    bezierSpline.UpdateControlPointLocal(i, newPos, SplinePoint.Pre);
                                    break;
                                }
                                case SplineEditMode.Free:
                                    bezierSpline.UpdateControlPointLocal(i, newPos, SplinePoint.Pre);
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

                            float2 newPoint = new float2(pos.x, pos.y) - origin.xy;
                            bezierSpline.UpdateControlPointLocal(i, newPoint, SplinePoint.Point);

                            if(m_editMoveWithTrans)
                            {
                                //move the other points relative to this
                                float2 delta = newPoint - point;
                                if(preExists)
                                    bezierSpline.UpdateControlPointLocal(i, bezierSpline.GetControlPoint(i, SplinePoint.Pre) + delta, SplinePoint.Pre);
                                if(postExists)
                                    bezierSpline.UpdateControlPointLocal(i, bezierSpline.GetControlPoint(i, SplinePoint.Post) + delta, SplinePoint.Post);
                            }

                            SceneView.RepaintAll();
                        }
                    }

                    if(postExists)
                    {
                        // post Point 
                        float2 post = bezierSpline.GetControlPoint2DWorld(i, SplinePoint.Post);
                        Vector3 pos = new Vector3(post.x, post.y, 0f);

                        Handles.DrawLine(pos, lastPos.Value);

                        EditorGUI.BeginChangeCheck();
                        pos = Handles.DoPositionHandle(pos, Quaternion.identity);
                        if(EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(bezierSpline, "Move Post Point");
                            EditorUtility.SetDirty(bezierSpline);

                            float2 newPos = new float2(pos.x, pos.y) - origin.xy;

                            switch (editMode)
                            {
                                case SplineEditMode.Standard:
                                {
                                    if(preExists)
                                    {
                                        float preMagnitude = math.length(point - bezierSpline.GetControlPoint(i, SplinePoint.Pre));
                                        UpdateOppositePoint(preMagnitude, point, newPos, i, SplinePoint.Pre);
                                    }

                                    bezierSpline.UpdateControlPointLocal(i, newPos, SplinePoint.Post);
                                    break;
                                }
                                case SplineEditMode.Mirror:
                                {
                                    if(preExists)
                                    {
                                        float postMagnitude = math.length(point - newPos);
                                        UpdateOppositePoint(postMagnitude, point, newPos, i, SplinePoint.Pre);
                                    }

                                    bezierSpline.UpdateControlPointLocal(i, newPos, SplinePoint.Post);
                                    break;
                                }
                                case SplineEditMode.Free:
                                    bezierSpline.UpdateControlPointLocal(i, newPos, SplinePoint.Post);
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
            bezierSpline.UpdateControlPointLocal(i, updatedPoint, pointType);
        }
    }
}