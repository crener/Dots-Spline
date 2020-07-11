using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Crener.Spline.Editor
{
    public static class EditorInputAbstractions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AddPointMode()
        {
            // this is abstracted so that it can be easily ported to the new input system if needed
            return Event.current.shift;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LeftClick()
        {
            // this is abstracted so that it can be easily ported to the new input system if needed

            Event e = Event.current;
            if(e.type != EventType.MouseDown) return false;
            return e.button == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 MousePos()
        {
            // this is abstracted so that it can be easily ported to the new input system if needed
            Vector3 handleResult = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            return new float2(handleResult.x, handleResult.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 MousePosFromSceneCamera()
        {
            // this is abstracted so that it can be easily ported to the new input system if needed
            Vector3 handleResult = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            return new float3(handleResult.x, handleResult.y, handleResult.y);
        }
    }
}