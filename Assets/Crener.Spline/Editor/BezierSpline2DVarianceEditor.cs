using System;
using System.Runtime.CompilerServices;
using Crener.Spline.BezierSpline;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor
{
    /// <summary>
    /// Editor for <see cref="BezierSpline2DSimple"/> which allows for adjusting control points and control point variance as required.
    /// </summary>
    [CustomEditor(typeof(BezierSpline2DVariance))]
    public class BezierSpline2DVarianceEditor : UnityEditor.Editor
    {
        private const int c_zebraMaxLines = 600;
        private const int c_zebraMaxLineResolution = 7;
        
        private static readonly Color s_handleSelected = Color.blue;
        private static readonly Color s_handleNotSelected = new Color(0f, 1f, 1f, 0.73f);
        private static readonly Color s_handleLeft = new Color(0.06f, 0.64f, 0f);
        private static readonly Color s_handleLeftDark = new Color(0.06f, 0.49f, 0f);
        private static readonly Color s_handleRight = new Color(0.75f, 0.16f, 0f);
        private static readonly Color s_handleRightDark = new Color(0.49f, 0.1f, 0f);
        private static readonly Color s_warningColour = new Color(0.99f, 0.77f, 0.14f);

        protected virtual ISimpleSpline2DVariance Spline
        {
            get => m_spline;
            set => m_spline = value;
        }
        protected virtual UnityEngine.Object SplineObject => m_spline as UnityEngine.Object;

        private ISimpleSpline2DVariance m_spline = null;
        private bool m_editing = false;
        private int? m_editControlPoint = null;
        private bool m_editMoveRelated = true;
        private Tool m_previousTool;
        private bool m_debugDrawLineEnabled = false;
        private float m_debugDrawLineVariance = 0f;
        private bool m_debugDrawZebraEnabled = false;
        private int m_debugDrawZebraDensity = 250;
        private int m_debugDrawZebraPoints = 2;

        /// <summary>
        /// Draws the inspector window GUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            if(target != SplineObject)
            {
                Spline = target as ISimpleSpline2DVariance;
                m_editing = false;
                m_editControlPoint = null;
                m_debugDrawZebraEnabled = false;

                m_previousTool = Tools.current;
                Tools.current = Tool.Custom;
            }

            if(Spline == null)
            {
                EditorGUILayout.LabelField($"Unknown Type inspected by {nameof(BezierSpline2DVarianceEditor)}");
                return;
            }

            if(GUILayout.Button("Edit Points"))
            {
                m_editing = !m_editing;
                m_editControlPoint = null;

                if(m_editing)
                {
                    m_previousTool = Tools.current == Tool.Custom ? Tool.Move : Tools.current;
                    Tools.current = Tool.None;
                }
                else
                {
                    Tools.current = m_previousTool;
                }

                SceneView.RepaintAll();
            }

            if(!m_editing)
            {
                GUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    $"Points: {Spline.ControlPointCount}\n" +
                    $"Length: {Spline.Length():N3} (" +
                    $"L: {Spline.Length(new half(-1f)):N3} " +
                    $"R: {Spline.Length(new half(1f)):N3})", MessageType.None);

                GUILayout.Space(5);

                // Draw Line - Draw a line that represents the path that an item would take when using the specified variance
                EditorGUI.BeginChangeCheck();
                m_debugDrawLineEnabled = EditorGUILayout.Toggle("Show Debug Draw Line", m_debugDrawLineEnabled);
                m_debugDrawLineVariance = EditorGUILayout.Slider("Line Variance", m_debugDrawLineVariance, -1f, 1f);
                if(EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }

                // Draw Zebra Stripes - This helps to debug issues where the spline doesn't match up correctly, which can happen when
                // reusing the spline editor inside other controls
                EditorGUI.BeginChangeCheck();
                m_debugDrawZebraEnabled = EditorGUILayout.Toggle("Show Debug Zebra Stripes", m_debugDrawZebraEnabled);
                m_debugDrawZebraDensity = EditorGUILayout.IntSlider("Density",m_debugDrawZebraDensity, 10, c_zebraMaxLines);
                m_debugDrawZebraPoints = EditorGUILayout.IntSlider("Lines per stripe",m_debugDrawZebraPoints, 1, c_zebraMaxLineResolution);
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
                    EditorGUILayout.HelpBox("No Control Point Selected!", MessageType.None);
                }
                else
                {
                    EditorGUILayout.LabelField($"Control Point: {m_editControlPoint}");

                    SplineEditMode existingValue = Spline.GetEditMode(m_editControlPoint.Value);
                    SplineEditMode editMode = (SplineEditMode) EditorGUILayout.EnumPopup("Edit Mode", existingValue);
                    if(editMode != existingValue)
                    {
                        Undo.RecordObject(SplineObject, $"Change EditMode of Point {m_editControlPoint}");
                        EditorUtility.SetDirty(SplineObject);
                        Spline.ChangeEditMode(m_editControlPoint.Value, editMode);

                        SceneView.RepaintAll();
                    }

                    GUILayout.Space(10);
                    if(GUILayout.Button("Remove Point") ||
                       Event.current.isKey && Event.current.type == EventType.KeyDown &&
                       Event.current.keyCode == KeyCode.Delete)
                    {
                        Spline.RemoveControlPoint(m_editControlPoint.Value);
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
        /// Draws scene controls
        /// </summary>
        protected void OnSceneGUI()
        {
            if(Event.current.type == EventType.Used) return;

            if(m_editing)
            {
                if(m_editControlPoint.HasValue && InputAbstractions.AddPointMode())
                    m_editControlPoint = null;

                RenderControlPoints();

                if(InputAbstractions.AddPointMode())
                {
                    CheckPointCreation();

                    if(Event.current.type == EventType.MouseMove)
                        SceneView.RepaintAll();
                }
            }
            else
            {
                if(m_debugDrawZebraEnabled)
                {
                    half center = new half(0);

                    Handles.color = s_handleLeftDark;
                    for (int i = 0; i < m_debugDrawZebraDensity; i++)
                    {
                        float progress = i / (float) m_debugDrawZebraDensity;

                        float2 a = Spline.GetPoint(progress, center);
                        for (float j = 1; j <= m_debugDrawZebraPoints; j++)
                        {
                            half left = new half(-(j / m_debugDrawZebraPoints));
                            float2 b = Spline.GetPoint(progress, left);
                            Handles.DrawLine(new Vector2(a.x, a.y), new Vector2(b.x, b.y));
                            a = b;
                        }
                    }

                    Handles.color = s_handleRightDark;
                    for (int i = 0; i < m_debugDrawZebraDensity; i++)
                    {
                        float progress = i / (float) m_debugDrawZebraDensity;
                        float2 a = Spline.GetPoint(progress, center);
                        for (float j = 1; j <= m_debugDrawZebraPoints; j++)
                        {
                            half right = new half(j / m_debugDrawZebraPoints);
                            float2 b = Spline.GetPoint(progress, right);
                            Handles.DrawLine(new Vector2(a.x, a.y), new Vector2(b.x, b.y));
                            a = b;
                        }
                    }
                }

                if(m_debugDrawLineEnabled)
                {
                    const float partDistance = 0.1f;
                    Color colour = new Color(1f, 0.18f, 0.54f);
                    Handles.color = colour;

                    half variance = new half(m_debugDrawLineVariance);
                    int points = (int) (Spline.Length(variance) / partDistance);

                    float2 f = Spline.GetPoint(0f, variance);
                    Vector3 lp = new Vector3(f.x, f.y, 0f);

                    for (int i = 1; i <= points; i++)
                    {
                        float progress = i / (float) points;
                        float2 p = Spline.GetPoint(progress, variance);
                        Vector3 point = new Vector3(p.x, p.y, 0f);

                        if(math.distance(lp, point) > partDistance * 1.3f)
                        {
                            Handles.color = s_warningColour;
                            Handles.DrawLine(lp, point);
                            Handles.color = colour;
                        }
                        else
                        {
                            Handles.DrawLine(lp, point);
                        }

                        lp = point;
                    }
                }
            }
        }

        /// <summary>
        /// Used to check and identify the location and index of a new point in the spline
        /// </summary>
        private void CheckPointCreation()
        {
            if(Spline.ControlPointCount < 2)
            {
                if(InputAbstractions.LeftClick())
                {
                    Vector3 pos = Input.mousePosition;
                    Spline.AddControlPoint(new float2(pos.x, pos.y));
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
                Undo.RecordObject(SplineObject, "Insert Spline Point");
                EditorUtility.SetDirty(SplineObject);

                Spline.InsertControlPoint(segmentIndex, mouse);

                SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// return the current closest point to <paramref name="mouse"/> on the spline at between <paramref name="index"/> - 1 and <paramref name="index"/>
        /// </summary>
        /// <param name="mouse">position to look for the closest point from</param>
        /// <param name="index">spline index to inspect</param>
        /// <returns>closest point to <paramref name="mouse"/> at <paramref name="index"/></returns>
        private float2 ClosestPointSelection(float2 mouse, out int index)
        {
            index = 0;

            float2 bestPoint = float2.zero;
            float bestDistance = float.MaxValue;

            for (int i = 1; i < Spline.ControlPointCount; i++)
            {
                for (int s = 0; s <= 64; s++)
                {
                    float progress = s / 64f;
                    float2 p = Spline.GetPoint(progress, i - 1, new half(0));

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
        /// Render the control point controls. The actual spline drawing is already performed by the selected spline itself.
        /// </summary>
        private void RenderControlPoints()
        {
            for (int i = 0; i < Spline.ControlPointCount; i++)
            {
                float2 point = Spline.GetControlPoint(i, SplinePointVariance.Point);
                Vector3 editorPosition = new Vector3(point.x, point.y);
                float handleSize = HandleUtility.GetHandleSize(editorPosition) / 12f;

                // draw handles
                if(m_editControlPoint == i)
                {
                    SplineEditMode editMode = Spline.GetEditMode(i);
                    float2 pointL = Spline.GetControlPoint(i, SplinePointVariance.PointLeft);
                    Vector3 mainL = new Vector3(pointL.x, pointL.y);
                    float2 pointR = Spline.GetControlPoint(i, SplinePointVariance.PointRight);
                    Vector3 mainR = new Vector3(pointR.x, pointR.y);

                    bool preExists = i > 0;
                    bool postExists = i != Spline.ControlPointCount - 1;

                    float mainAngle = 0f, preAngle = 0f, postAngle = 0f;
                    CalculateAngles(ref mainAngle, ref preAngle, ref postAngle, editorPosition, preExists, postExists, i);

                    if(preExists)
                    {
                        bool2 pointExistence = new bool2(true, postExists);

                        Handles.color = s_handleSelected;
                        if(HandleRect(i, SplinePointVariance.Pre, editMode, pointExistence, editorPosition, point,
                            "Move Pre Point", out float moveDelta))
                        {
                            // make sure to recalculate angles to avoid "lag" in derived control point positions
                            pointL = Spline.GetControlPoint(i, SplinePointVariance.PointLeft); 
                            CalculateAngles(ref mainAngle, ref preAngle, ref postAngle, editorPosition, true, postExists, i);

                            float2 sideDelta = new float2(moveDelta, 0f);
                            UpdatePointSides(point, pointL, pointL, i, editMode, SplinePointVariance.PointLeft,
                                postExists, postAngle, true, preAngle, mainAngle - (math.PI / 2f), sideDelta);
                            UpdatePointSides(point, pointR, pointR, i, editMode, SplinePointVariance.PointRight,
                                postExists, postAngle, true, preAngle, mainAngle + (math.PI / 2f), sideDelta);
                        }

                        Handles.color = s_handleLeft;
                        HandleRectSlider(i, SplinePointVariance.PreLeft, editMode, pointExistence, mainL, point,
                            "Move Pre Point Left", preAngle, postAngle);

                        Handles.color = s_handleRight;
                        HandleRectSlider(i, SplinePointVariance.PreRight, editMode, pointExistence, mainR, point,
                            "Move Pre Point Right", preAngle, postAngle);
                    }

                    {
                        Handles.color = s_handleSelected;

                        #region main point
                        if(ShouldShowPoint(i, SplinePointVariance.Point))
                        {
                            if(!AllowPointMovement(i, SplinePointVariance.Point))
                            {
                                Handles.Button(editorPosition, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap);
                            }
                            else
                            {
                                EditorGUI.BeginChangeCheck();
                                Vector3 pos = Handles.DoPositionHandle(editorPosition, Quaternion.identity);
                                if(EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(SplineObject, "Move Point");
                                    EditorUtility.SetDirty(SplineObject);

                                    float2 newPoint = new float2(pos.x, pos.y);
                                    Spline.UpdateControlPoint(i, newPoint, SplinePointVariance.Point);

                                    float2 delta = newPoint - point;

                                    // move the left and right points
                                    Spline.UpdateControlPoint(i, Spline.GetControlPoint(i, SplinePointVariance.PointLeft) + delta,
                                        SplinePointVariance.PointLeft);
                                    Spline.UpdateControlPoint(i, Spline.GetControlPoint(i, SplinePointVariance.PointRight) + delta,
                                        SplinePointVariance.PointRight);

                                    if(m_editMoveRelated)
                                    {
                                        // move the other points relative to this
                                        if(preExists)
                                        {
                                            Spline.UpdateControlPoint(i, Spline.GetControlPoint(i, SplinePointVariance.Pre) + delta,
                                                SplinePointVariance.Pre);
                                            Spline.UpdateControlPoint(i, Spline.GetControlPoint(i, SplinePointVariance.PreLeft) + delta,
                                                SplinePointVariance.PreLeft);
                                            Spline.UpdateControlPoint(i, Spline.GetControlPoint(i, SplinePointVariance.PreRight) + delta,
                                                SplinePointVariance.PreRight);
                                        }

                                        if(postExists)
                                        {
                                            Spline.UpdateControlPoint(i, Spline.GetControlPoint(i, SplinePointVariance.Post) + delta,
                                                SplinePointVariance.Post);
                                            Spline.UpdateControlPoint(i, Spline.GetControlPoint(i, SplinePointVariance.PostLeft) + delta,
                                                SplinePointVariance.PostLeft);
                                            Spline.UpdateControlPoint(i, Spline.GetControlPoint(i, SplinePointVariance.PostRight) + delta,
                                                SplinePointVariance.PostRight);
                                        }
                                    }

                                    SceneView.RepaintAll();
                                }
                            }
                        }
                        #endregion

                        #region point left
                        Handles.color = s_handleLeft;
                        if(ShouldShowPoint(i, SplinePointVariance.PointLeft))
                        {
                            if(!AllowPointMovement(i, SplinePointVariance.PointLeft))
                            {
                                Handles.Button(editorPosition, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap);
                            }
                            else
                            {
                                EditorGUI.BeginChangeCheck();
                                if(editMode != SplineEditMode.Free)
                                {
                                    mainL = Handles.Slider(mainL, new Vector3(
                                        math.sin(mainAngle + (math.PI / 2f)),
                                        math.cos(mainAngle + (math.PI / 2f)), 0f));
                                    if(EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(SplineObject, "Move Left Point");

                                        // drag the pre and post points along
                                        float2 newPoint = new float2(mainL.x, mainL.y);
                                        UpdatePointSides(point, pointL, newPoint, i, editMode, SplinePointVariance.PointLeft,
                                            postExists, postAngle, preExists, preAngle, mainAngle - (math.PI / 2f));
                                        pointL = newPoint;
                                        SceneView.RepaintAll();
                                    }
                                }
                                else
                                {
                                    mainL = Handles.DoPositionHandle(mainL, Quaternion.identity);
                                    if(EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(SplineObject, "Move Left Point");

                                        // drag the pre and post points along 
                                        float2 newPoint = new float2(mainL.x, mainL.y);
                                        UpdatePointSides(point, pointL, newPoint, i, editMode, SplinePointVariance.PointLeft,
                                            postExists, postAngle, preExists, preAngle, mainAngle - (math.PI / 2f));
                                        pointL = newPoint;
                                        SceneView.RepaintAll();
                                    }
                                }
                            }
                        }
                        #endregion

                        #region point right
                        Handles.color = s_handleRight;
                        if(ShouldShowPoint(i, SplinePointVariance.PointRight))
                        {
                            if(!AllowPointMovement(i, SplinePointVariance.PointRight))
                            {
                                Handles.Button(editorPosition, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap);
                            }
                            else
                            {
                                EditorGUI.BeginChangeCheck();
                                if(editMode != SplineEditMode.Free)
                                {
                                    mainR = Handles.Slider(mainR, new Vector3(
                                        -math.sin(mainAngle + (math.PI / 2f)),
                                        -math.cos(mainAngle + (math.PI / 2f)), 0f));
                                    if(EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(SplineObject, "Move Right Point");

                                        // drag the pre and post points along
                                        float2 newPoint = new float2(mainR.x, mainR.y);
                                        UpdatePointSides(point, pointR, newPoint, i, editMode, SplinePointVariance.PointRight,
                                            postExists, postAngle, preExists, preAngle, mainAngle + (math.PI / 2f));
                                        pointR = newPoint;
                                        SceneView.RepaintAll();
                                    }
                                }
                                else
                                {
                                    mainR = Handles.DoPositionHandle(mainR, Quaternion.identity);
                                    if(EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(SplineObject, "Move Right Point");

                                        // drag the pre and post points along
                                        float2 newPoint = new float2(mainR.x, mainR.y);
                                        UpdatePointSides(point, pointR, newPoint, i, editMode, SplinePointVariance.PointRight,
                                            postExists, postAngle, preExists, preAngle, mainAngle + (math.PI / 2f));
                                        pointR = newPoint;
                                        SceneView.RepaintAll();
                                    }
                                }
                            }
                        }
                        #endregion
                    }

                    if(postExists)
                    {
                        bool2 pointExistence = new bool2(preExists, true);

                        Handles.color = s_handleSelected;
                        if(HandleRect(i, SplinePointVariance.Post, editMode, pointExistence, editorPosition, point,
                            "Move Post Point", out float moveDelta))
                        {
                            // make sure to recalculate angles to avoid "lag" in derived control point positions
                            pointL = Spline.GetControlPoint(i, SplinePointVariance.PointLeft);
                            CalculateAngles(ref mainAngle, ref preAngle, ref postAngle, editorPosition, preExists, true, i);

                            float2 sideDelta = new float2(0f, moveDelta);
                            UpdatePointSides(point, pointR, pointR, i, editMode, SplinePointVariance.PointRight,
                                true, postAngle, preExists, preAngle, mainAngle + (math.PI / 2f), sideDelta);
                            UpdatePointSides(point, pointL, pointL, i, editMode, SplinePointVariance.PointLeft,
                                true, postAngle, preExists, preAngle, mainAngle - (math.PI / 2f), sideDelta);
                        }

                        Handles.color = s_handleLeft;
                        HandleRectSlider(i, SplinePointVariance.PostLeft, editMode, pointExistence, mainL, point,
                            "Move Post Point Left", preAngle, postAngle);

                        Handles.color = s_handleRight;
                        HandleRectSlider(i, SplinePointVariance.PostRight, editMode, pointExistence, mainR, point,
                            "Move Post Point Right", preAngle, postAngle);
                    }
                }
                else
                {
                    Handles.color = s_handleNotSelected;
                    if(Handles.Button(editorPosition, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap))
                    {
                        m_editControlPoint = i;
                        Repaint();
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the control points angles between the available pre, post and main points
        /// </summary>
        /// <param name="mainAngle">reference to the primary angle of the control point</param>
        /// <param name="preAngle">angle of the pre control points</param>
        /// <param name="postAngle">angle of the post control points</param>
        /// <param name="editorPosition">the position of the center control point</param>
        /// <param name="preExists">true if pre control points exist</param>
        /// <param name="postExists">true if post control points exist</param>
        /// <param name="i">index of the control point in the spline</param>
        private void CalculateAngles(ref float mainAngle, ref float preAngle, ref float postAngle, Vector3 editorPosition,
            bool preExists, bool postExists, int i)
        {
            if(preExists)
            {
                float2 pre = Spline.GetControlPoint(i, SplinePointVariance.Pre);
                mainAngle = preAngle = math.atan2(editorPosition.x - pre.x, editorPosition.y - pre.y);
            }

            if(postExists)
            {
                float2 post = Spline.GetControlPoint(i, SplinePointVariance.Post);
                mainAngle = postAngle = math.atan2(post.x - editorPosition.x, post.y - editorPosition.y);
            }

            if(preExists && postExists)
            {
                mainAngle = (preAngle + postAngle) / 2f;
            }
        }

        /// <summary>
        /// Handle the drawing of a selected spline control point
        /// </summary>
        /// <param name="i">control point index</param>
        /// <param name="pointType">type of point to handle rect for</param>
        /// <param name="editMode">edit mode of the control point</param>
        /// <param name="pointsExist">booleans for pre (X axis) and post (Y axis) existence</param>
        /// <param name="lineSource">the center point as a vector3</param>
        /// <param name="centerPoint">the center of the control point</param>
        /// <param name="undoMessage">message for Unity undo functionality</param>
        /// <returns>true if point was modified</returns>
        private bool HandleRect(int i, SplinePointVariance pointType, SplineEditMode editMode, bool2 pointsExist,
            Vector3 lineSource, float2 centerPoint, string undoMessage)
        {
            float moveDelta;
            return HandleRect(i, pointType, editMode, pointsExist, lineSource, centerPoint, undoMessage, out moveDelta);
        }

        /// <summary>
        /// Handle the drawing of a selected spline control point
        /// </summary>
        /// <param name="i">control point index</param>
        /// <param name="pointType">type of point to handle rect for</param>
        /// <param name="editMode">edit mode of the control point</param>
        /// <param name="pointsExist">booleans for pre (X axis) and post (Y axis) existence</param>
        /// <param name="lineSource">the center point as a vector3</param>
        /// <param name="centerPoint">the center of the control point</param>
        /// <param name="undoMessage">message for Unity undo functionality</param>
        /// <param name="moveDelta">total distance between the old point and the new point</param>
        /// <returns>true if point was modified</returns>
        private bool HandleRect(int i, SplinePointVariance pointType, SplineEditMode editMode, bool2 pointsExist,
            Vector3 lineSource, float2 centerPoint, string undoMessage, out float moveDelta)
        {
            moveDelta = 0f;
            if(!ShouldShowPoint(i, pointType)) return false;

            float2 controlPoint = Spline.GetControlPoint(i, pointType);
            Vector3 pos = new Vector3(controlPoint.x, controlPoint.y, 0f);

            Handles.DrawLine(pos, lineSource);
            if(!AllowPointMovement(i, pointType)) return false;

            EditorGUI.BeginChangeCheck();
            pos = Handles.DoPositionHandle(pos, Quaternion.identity);
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SplineObject, undoMessage);
                EditorUtility.SetDirty(SplineObject);

                float2 newPos = new float2(pos.x, pos.y);
                moveDelta = math.length(centerPoint - newPos) - math.length(centerPoint - controlPoint);

                if(pointType == SplinePointVariance.Pre)
                {
                    UpdatePointPre(centerPoint, newPos, i, editMode, pointType, pointsExist.y);
                }
                else if(pointType == SplinePointVariance.Post)
                {
                    UpdatePointPost(centerPoint, newPos, i, editMode, pointType, pointsExist.x);
                }
                else if(pointType == SplinePointVariance.PreLeft ||
                        pointType == SplinePointVariance.PreRight)
                {
                    UpdatePointPre(new float2(lineSource.x, lineSource.y), newPos, i, editMode, pointType, pointsExist.y);
                }
                else if(pointType == SplinePointVariance.PostLeft ||
                        pointType == SplinePointVariance.PostRight)
                {
                    UpdatePointPost(new float2(lineSource.x, lineSource.y), newPos, i, editMode, pointType, pointsExist.x);
                }
                else
                {
                    throw new ArgumentException("Unsupported Variance point encountered! " + pointType, nameof(pointType));
                }

                SceneView.RepaintAll();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle the drawing of a selected spline control point
        /// </summary>
        /// <param name="i">control point index</param>
        /// <param name="pointType">type of point to handle rect for</param>
        /// <param name="editMode">edit mode of the control point</param>
        /// <param name="pointsExist">booleans for pre (X axis) and post (Y axis) existence</param>
        /// <param name="lineSource">the center point as a vector3</param>
        /// <param name="centerPoint">the center of the control point</param>
        /// <param name="undoMessage">message for Unity undo functionality</param>
        /// <param name="preAngle">pre angle for the control point</param>
        /// <param name="postAngle">post angle for the control point</param>
        /// <returns>true if point was modified</returns>
        private bool HandleRectSlider(int i, SplinePointVariance pointType, SplineEditMode editMode, bool2 pointsExist,
            Vector3 lineSource, float2 centerPoint, string undoMessage, float preAngle, float postAngle)
        {
            if(!ShouldShowPoint(i, pointType)) return false;
            if(editMode == SplineEditMode.Free)
            {
                return HandleRect(i, pointType, editMode, pointsExist, lineSource, centerPoint, undoMessage);
            }

            float2 controlPoint = Spline.GetControlPoint(i, pointType);
            Vector3 pos = new Vector3(controlPoint.x, controlPoint.y, 0f);
            float angle = pointType.isPre() ? preAngle + math.PI : postAngle;

            //draw visualisation line
            Handles.DrawLine(pos, lineSource);
            const float lengthDistance = 0.3f;
            Vector3 posOffset = lineSource + (new Vector3(math.sin(angle), math.cos(angle), 0f)
                                              * (math.length(lineSource - pos) + (lengthDistance * 0.45f)));
            Handles.DrawLine(
                posOffset + (new Vector3(math.sin(angle + 90f), math.cos(angle + 90f), 0f) * lengthDistance),
                posOffset + (new Vector3(math.sin(angle - 90f), math.cos(angle - 90f), 0f) * lengthDistance));

            if(!AllowPointMovement(i, pointType)) return false;

            EditorGUI.BeginChangeCheck();
            pos = Handles.Slider(pos, new Vector3(math.sin(angle), math.cos(angle), 0f));
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SplineObject, undoMessage);
                EditorUtility.SetDirty(SplineObject);

                float2 newPos = new float2(pos.x, pos.y);

                if(pointType == SplinePointVariance.Pre)
                {
                    UpdatePointPre(centerPoint, newPos, i, editMode, pointType, pointsExist.y);
                }
                else if(pointType == SplinePointVariance.Post)
                {
                    UpdatePointPost(centerPoint, newPos, i, editMode, pointType, pointsExist.x);
                }
                else if(pointType == SplinePointVariance.PreLeft ||
                        pointType == SplinePointVariance.PreRight)
                {
                    UpdatePointPre(new float2(lineSource.x, lineSource.y), newPos, i, editMode, pointType, pointsExist.y);
                }
                else if(pointType == SplinePointVariance.PostLeft ||
                        pointType == SplinePointVariance.PostRight)
                {
                    UpdatePointPost(new float2(lineSource.x, lineSource.y), newPos, i, editMode, pointType, pointsExist.x);
                }
                else
                {
                    throw new ArgumentException("Unsupported Variance point encountered! " + pointType, nameof(pointType));
                }

                SceneView.RepaintAll();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update a pre point position data in the spline
        /// </summary>
        /// <param name="center">center point of the control point</param>
        /// <param name="newPoint">new point location</param>
        /// <param name="cpIndex">control point index</param>
        /// <param name="editMode">edit mode of the control point</param>
        /// <param name="pointType">type of point to handle</param>
        /// <param name="postExists">post point available for the control point</param>
        private void UpdatePointPre(float2 center, float2 newPoint, int cpIndex, SplineEditMode editMode, SplinePointVariance pointType,
            bool postExists)
        {
            switch (editMode)
            {
                case SplineEditMode.Standard:
                {
                    if(postExists)
                    {
                        float postMagnitude = math.length(center - Spline.GetControlPoint(cpIndex, pointType.OppositePrePost()));
                        UpdateOppositePoint(postMagnitude, center, newPoint, cpIndex, pointType.OppositePrePost());
                    }

                    Spline.UpdateControlPoint(cpIndex, newPoint, pointType);
                    break;
                }
                case SplineEditMode.Mirror:
                {
                    if(postExists)
                    {
                        float preMagnitude = math.length(center - newPoint);
                        UpdateOppositePoint(preMagnitude, center, newPoint, cpIndex, pointType.OppositePrePost());
                    }

                    Spline.UpdateControlPoint(cpIndex, newPoint, pointType);
                    break;
                }
                case SplineEditMode.Free:
                    Spline.UpdateControlPoint(cpIndex, newPoint, pointType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Update a post point position data in the spline
        /// </summary>
        /// <param name="center">center point of the control point</param>
        /// <param name="newPoint">new point location</param>
        /// <param name="cpIndex">control point index</param>
        /// <param name="editMode">edit mode of the control point</param>
        /// <param name="pointType">type of point to handle</param>
        /// <param name="preExists">pre point available for the control point</param>
        private void UpdatePointPost(float2 center, float2 newPoint, int cpIndex, SplineEditMode editMode, SplinePointVariance pointType,
            bool preExists)
        {
            switch (editMode)
            {
                case SplineEditMode.Standard:
                {
                    if(preExists)
                    {
                        float preMagnitude = math.length(center - Spline.GetControlPoint(cpIndex, pointType.OppositePrePost()));
                        UpdateOppositePoint(preMagnitude, center, newPoint, cpIndex, pointType.OppositePrePost());
                    }

                    Spline.UpdateControlPoint(cpIndex, newPoint, pointType);
                    break;
                }
                case SplineEditMode.Mirror:
                {
                    if(preExists)
                    {
                        float postMagnitude = math.length(center - newPoint);
                        UpdateOppositePoint(postMagnitude, center, newPoint, cpIndex, pointType.OppositePrePost());
                    }

                    Spline.UpdateControlPoint(cpIndex, newPoint, pointType);
                    break;
                }
                case SplineEditMode.Free:
                    Spline.UpdateControlPoint(cpIndex, newPoint, pointType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Update a pre point left position data in the spline and adjust the related points
        /// </summary>
        /// <param name="mainPoint">center point inside the control point</param>
        /// <param name="oldPoint">old point position</param>
        /// <param name="newPoint">new point position to update too</param>
        /// <param name="cpIndex">control point index</param>
        /// <param name="editMode">edit mode of the control point</param>
        /// <param name="splineType">type of point to update</param>
        /// <param name="postExists">post point available for the control point</param>
        /// <param name="postAngle">post angle for the control point</param>
        /// <param name="preExists">pre point available for the control point</param>
        /// <param name="preAngle">pre angle for the control point</param>
        /// <param name="mainAngle">angle between the post and pre angle</param>
        private void UpdatePointSides(float2 mainPoint, float2 oldPoint, float2 newPoint, int cpIndex, SplineEditMode editMode,
            SplinePointVariance splineType, bool postExists, float postAngle, bool preExists, float preAngle, float mainAngle)
        {
            UpdatePointSides(mainPoint, oldPoint, newPoint, cpIndex, editMode, splineType, 
                postExists, postAngle, preExists, preAngle, mainAngle, float2.zero);
        }

        /// <summary>
        /// Update a pre point left position data in the spline and adjust the related points
        /// </summary>
        /// <param name="mainPoint">center point inside the control point</param>
        /// <param name="oldPoint">old point position</param>
        /// <param name="newPoint">new point position to update too</param>
        /// <param name="cpIndex">control point index</param>
        /// <param name="editMode">edit mode of the control point</param>
        /// <param name="splineType">type of point to update</param>
        /// <param name="postExists">post point available for the control point</param>
        /// <param name="postAngle">post angle for the control point</param>
        /// <param name="preExists">pre point available for the control point</param>
        /// <param name="preAngle">pre angle for the control point</param>
        /// <param name="mainAngle">angle between the post and pre angle</param>
        /// <param name="additionalDelta">optional: additional delta to add to the sides</param>
        private void UpdatePointSides(float2 mainPoint, float2 oldPoint, float2 newPoint, int cpIndex, SplineEditMode editMode,
            SplinePointVariance splineType, bool postExists, float postAngle, bool preExists, float preAngle, float mainAngle,
            float2 additionalDelta)
        {
            switch (editMode)
            {
                case SplineEditMode.Standard:
                {
                    float newMagnitude = math.length(mainPoint - newPoint);
                    float2 newSide = mainPoint + (new float2(
                                                      -math.sin(mainAngle),
                                                      -math.cos(mainAngle)) * newMagnitude);

                    Spline.UpdateControlPoint(cpIndex, newSide, splineType);

                    if(preExists)
                    {
                        SplinePointVariance pre = splineType.ToPre();
                        float2 controlPoint = Spline.GetControlPoint(cpIndex, pre);
                        float preMagnitude = math.length(oldPoint - controlPoint) + additionalDelta.x;
                        float2 point = newSide + (new float2(
                                                      -math.sin(preAngle),
                                                      -math.cos(preAngle)) * preMagnitude);

                        Spline.UpdateControlPoint(cpIndex, point, pre);
                    }
                    
                    if(postExists)
                    {
                        //Update the post position
                        SplinePointVariance post = splineType.ToPost();
                        float2 controlPoint = Spline.GetControlPoint(cpIndex, post);
                        float postMagnitude = math.length(oldPoint - controlPoint) + additionalDelta.y;
                        float2 point = newSide + (new float2(
                                                      math.sin(postAngle),
                                                      math.cos(postAngle)) * postMagnitude);

                        Spline.UpdateControlPoint(cpIndex, point, post);
                    }

                    break;
                }
                case SplineEditMode.Mirror:
                {
                    // update this point
                    UpdatePointSides(mainPoint, oldPoint, newPoint, cpIndex, SplineEditMode.Standard, splineType,
                        postExists, postAngle, preExists, preAngle, mainAngle, additionalDelta);

                    // mirror changes in the other side
                    SplinePointVariance opposite = splineType.OppositeLR();
                    float angle = math.atan2(mainPoint.x - oldPoint.x, mainPoint.y - oldPoint.y) + math.PI;

                    float newMagnitude = math.length(mainPoint - newPoint);
                    float2 newSide = mainPoint + (new float2(math.sin(angle), math.cos(angle)) * newMagnitude);

                    float2 originalPoint = Spline.GetControlPoint(cpIndex, opposite);
                    UpdatePointSides(mainPoint, originalPoint, newSide, cpIndex, SplineEditMode.Standard, opposite,
                        postExists, postAngle, preExists, preAngle, mainAngle + (math.PI));
                    break;
                }
                case SplineEditMode.Free:
                {
                    Spline.UpdateControlPoint(cpIndex, newPoint, splineType);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Updates point based on given <paramref name="magnitude"/>
        /// </summary>
        /// <param name="magnitude">distance from the center to the point</param>
        /// <param name="center">center to pivot around</param>
        /// <param name="newPos">new point position</param>
        /// <param name="i">spline control point index</param>
        /// <param name="pointType">point type to update with newly computed position</param>
        private void UpdateOppositePoint(float magnitude, float2 center, float2 newPos, int i, SplinePointVariance pointType)
        {
            float2 delta = center - newPos;
            float angle = math.atan2(delta.y, delta.x) - (math.PI / 2f);

            // match the opposite angle for the pointType
            float2 updatedPoint = new float2(
                center.x + (math.sin(-angle) * magnitude),
                center.y + (math.cos(-angle) * magnitude));
            Spline.UpdateControlPoint(i, updatedPoint, pointType);
        }

        /// <summary>
        /// Check if the spline editor should show the spline
        /// </summary>
        /// <param name="index">index that will be modified</param>
        /// <param name="pointType">point type that will be modified</param>
        /// <returns>true if user should be allowed to edit</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool ShouldShowPoint(int index, SplinePointVariance pointType)
        {
            return true;
        }

        /// <summary>
        /// Check if the spline editor should allow the user to change the current spline point
        /// </summary>
        /// <param name="index">index that will be modified</param>
        /// <param name="pointType">point type that will be modified</param>
        /// <returns>true if user should be allowed to edit</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool AllowPointMovement(int index, SplinePointVariance pointType)
        {
            return true;
        }
    }
}