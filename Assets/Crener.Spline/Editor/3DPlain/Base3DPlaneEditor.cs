using Crener.Spline.Common;
using Crener.Spline.Common.Interfaces;
using Crener.Spline.Editor._3D;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor._3DPlain
{
    public class Base3DPlaneEditor : Base3DEditor
    {
        // Override so that inspector point is shown as 2D
        protected override bool m_inspector3dPoint => false;
        
        /// <inheritdoc cref="Base3DEditor.DrawSelectedHandles"/>
        protected override void DrawSelectedHandles(ISpline3DEditor spline, int pointIndex)
        {
            float3 point = spline.GetControlPoint3D(pointIndex);

            EditorGUI.BeginChangeCheck();

            ISpline3DPlane plane = spline as ISpline3DPlane;
            Vector3 pos = Handles.DoPositionHandle(point, plane.Forward);

            if(EditorGUI.EndChangeCheck() && spline is Object objSpline)
            {
                Undo.RecordObject(objSpline, "Move Point");
                EditorUtility.SetDirty(objSpline);

                spline.UpdateControlPoint(pointIndex, pos, SplinePoint.Point);

                SceneView.RepaintAll();
            }
        }
        
        protected override void MoveWithTransform(ISpline3DEditor spline)
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
    }
}