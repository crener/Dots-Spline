using System;
using Crener.Spline.BezierSpline;
using Crener.Spline.Common;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Crener.Spline.Editor._3D
{
    /// <summary>
    /// Editor for <see cref="BezierSpline3DSimple"/> which allows for adjusting control points.
    /// </summary>
    [CustomEditor(typeof(BezierSpline3DSimple))]
    public class BezierSpline3DSimpleEditor : Base3DEditor
    {
        private BezierSpline3DSimple bezierSpline = null;

        public override void OnInspectorGUI()
        {
            if(target != bezierSpline)
            {
                bezierSpline = (BezierSpline3DSimple) target;
                ChangeTransform(bezierSpline.transform);
                m_editing = false;
                m_editControlPoint = null;
            }

            if(bezierSpline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by '{nameof(BezierSpline3DSimpleEditor)}'");
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

            CheckActiveCamera();
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

                if(selected) Handles.color = Color.blue;
                else Handles.color = new Color(0f, 1f, 1f, 0.73f);
                
                float3 point = bezierSpline.GetControlPoint3DWorld(i, SplinePoint.Point);
                Vector3 editorPosition = new Vector3(point.x, point.y, point.z);
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
                        float3 pre = bezierSpline.GetControlPoint3DWorld(i, SplinePoint.Pre);
                        Vector3 pos = new Vector3(pre.x, pre.y, pre.z);
                        lastPos = pos;

                        EditorGUI.BeginChangeCheck();
                        pos = Handles.DoPositionHandle(pos, Quaternion.identity);
                        if(EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(bezierSpline, "Move Pre Point");
                            EditorUtility.SetDirty(bezierSpline);

                            float3 newPos = new float3(pos.x, pos.y, pos.z);

                            switch (editMode)
                            {
                                case SplineEditMode.Standard:
                                {
                                    if(postExists)
                                    {
                                        float postMagnitude = math.length(point - bezierSpline.GetControlPoint3DWorld(i, SplinePoint.Post));
                                        UpdateOppositePoint(postMagnitude, point, newPos, i, SplinePoint.Post);
                                    }

                                    bezierSpline.UpdateControlPointWorld(i, newPos, SplinePoint.Pre);
                                    break;
                                }
                                case SplineEditMode.Mirror:
                                {
                                    if(postExists)
                                    {
                                        float preMagnitude = math.length(point - newPos);
                                        UpdateOppositePoint(preMagnitude, point, newPos, i, SplinePoint.Post);
                                    }

                                    bezierSpline.UpdateControlPointWorld(i, newPos, SplinePoint.Pre);
                                    break;
                                }
                                case SplineEditMode.Free:
                                    bezierSpline.UpdateControlPointWorld(i, newPos, SplinePoint.Pre);
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

                            float3 newPoint = new float3(pos.x, pos.y, pos.z);
                            bezierSpline.UpdateControlPointWorld(i, newPoint, SplinePoint.Point);

                            if(m_editMoveWithTrans)
                            {
                                //move the other points relative to this
                                float3 delta = newPoint - point;
                                
                                void UpdatePoint(SplinePoint type)
                                {
                                    float3 cp = bezierSpline.GetControlPoint3DWorld(i, type);
                                    bezierSpline.UpdateControlPointWorld(i, cp + delta, type);
                                }

                                if(preExists) UpdatePoint(SplinePoint.Pre);
                                if(postExists) UpdatePoint(SplinePoint.Post);
                            }

                            SceneView.RepaintAll();
                        }
                    }

                    if(postExists)
                    {
                        // post Point 
                        float3 post = bezierSpline.GetControlPoint3DWorld(i, SplinePoint.Post);
                        Vector3 pos = new Vector3(post.x, post.y, post.z);

                        Handles.DrawLine(pos, lastPos.Value);

                        EditorGUI.BeginChangeCheck();
                        pos = Handles.DoPositionHandle(pos, Quaternion.identity);
                        if(EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(bezierSpline, "Move Post Point");
                            EditorUtility.SetDirty(bezierSpline);

                            float3 newPos = new float3(pos.x, pos.y, pos.z);

                            switch (editMode)
                            {
                                case SplineEditMode.Standard:
                                {
                                    if(preExists)
                                    {
                                        float preMagnitude = math.length(point - bezierSpline.GetControlPoint3DWorld(i, SplinePoint.Pre));
                                        UpdateOppositePoint(preMagnitude, point, newPos, i, SplinePoint.Pre);
                                    }

                                    bezierSpline.UpdateControlPointWorld(i, newPos, SplinePoint.Post);
                                    break;
                                }
                                case SplineEditMode.Mirror:
                                {
                                    if(preExists)
                                    {
                                        float postMagnitude = math.length(point - newPos);
                                        UpdateOppositePoint(postMagnitude, point, newPos, i, SplinePoint.Pre);
                                    }

                                    bezierSpline.UpdateControlPointWorld(i, newPos, SplinePoint.Post);
                                    break;
                                }
                                case SplineEditMode.Free:
                                    bezierSpline.UpdateControlPointWorld(i, newPos, SplinePoint.Post);
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
        private void UpdateOppositePoint(float magnitude, float3 center, float3 newPos, int i, SplinePoint pointType)
        {
            float3 delta = center - newPos;
            
            Vector3 up = Vector3.Cross(Vector3.right, Vector3.Cross(delta, Vector3.up));
            Quaternion rotation = Quaternion.LookRotation(delta, up);
            Vector3 dir = ((rotation * Vector3.forward) * magnitude);

            // match the opposite angle for the pointType
            float3 updatedPoint = new float3(
                center.x + dir.x,
                center.y + dir.y,
                center.z + dir.z);
            bezierSpline.UpdateControlPointWorld(i, updatedPoint, pointType);
        }
    }
}