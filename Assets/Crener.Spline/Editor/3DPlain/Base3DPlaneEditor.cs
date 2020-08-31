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
        /// <inheritdoc cref="Base3DEditor.DrawSelectedHandles"/>
        protected override void DrawSelectedHandles(ISpline3DEditor spline, int pointIndex)
        {
            float3 point = spline.GetControlPoint3D(pointIndex);

            EditorGUI.BeginChangeCheck();

            Vector3 pos = Handles.DoPositionHandle(point, (spline as ISpline3DPlane).Forward);

            if(EditorGUI.EndChangeCheck() && spline is Object objSpline)
            {
                Undo.RecordObject(objSpline, "Move Point");
                EditorUtility.SetDirty(objSpline);

                float3 newPoint = new float3(pos.x, pos.y, pos.z);
                spline.UpdateControlPoint(pointIndex, newPoint, SplinePoint.Point);

                SceneView.RepaintAll();
            }
        }
    }
}