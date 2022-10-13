// Decompiled with JetBrains decompiler
// Type: UnityEditorInternal.Slider2D
// Assembly: UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DF1A03EC-30C1-4637-9D80-3C850C515D48
// Assembly location: E:\UnityEditor\2021.3.4f1c1\Editor\Data\Managed\UnityEngine\UnityEditor.CoreModule.dll

using UnityEditor;
using UnityEngine;

namespace LIBII.Light2D
{
    internal class Slider2D
    {
        private static Vector2 s_CurrentMousePosition;
        private static Vector3 s_StartPosition;
        private static Vector2 s_StartPlaneOffset;

        public static Vector3 Do(
            int id,
            Vector3 handlePos,
            Vector3 handleDir,
            Vector3 slideDir1,
            Vector3 slideDir2,
            float handleSize,
            Handles.CapFunction capFunction,
            float snap,
            bool drawHelper)
        {
            return Slider2D.Do(id, handlePos, new Vector3(0.0f, 0.0f, 0.0f), handleDir, slideDir1, slideDir2,
                handleSize, capFunction, new Vector2(snap, snap), drawHelper);
        }

        public static Vector3 Do(
            int id,
            Vector3 handlePos,
            Vector3 offset,
            Vector3 handleDir,
            Vector3 slideDir1,
            Vector3 slideDir2,
            float handleSize,
            Handles.CapFunction capFunction,
            float snap,
            bool drawHelper)
        {
            return Slider2D.Do(id, handlePos, offset, handleDir, slideDir1, slideDir2, handleSize, capFunction,
                new Vector2(snap, snap), drawHelper);
        }

        public static Vector3 Do(
            int id,
            Vector3 handlePos,
            Vector3 offset,
            Vector3 handleDir,
            Vector3 slideDir1,
            Vector3 slideDir2,
            float handleSize,
            Handles.CapFunction capFunction,
            Vector2 snap,
            bool drawHelper)
        {
            bool changed = GUI.changed;
            GUI.changed = false;
            Vector2 vector2 = Slider2D.CalcDeltaAlongDirections(id, handlePos, offset, handleDir, slideDir1, slideDir2,
                handleSize, capFunction, snap, drawHelper);
            if (GUI.changed)
            {
                handlePos = Slider2D.s_StartPosition + slideDir1 * vector2.x + slideDir2 * vector2.y;
                Vector3 vector3 = Vector3.Cross(slideDir1, slideDir2);
                if (IsCardinalDirection(vector3))
                    handlePos = Handles.inverseMatrix.MultiplyPoint(Snapping.Snap(
                        Handles.matrix.MultiplyPoint(handlePos), Vector3.one,
                        (SnapAxis) ~new SnapAxisFilter(vector3)));
            }

            GUI.changed |= changed;
            return handlePos;
        }

        static bool IsCardinalDirection(Vector3 direction) =>
            (double) Mathf.Abs(direction.x) > 0.0 && Mathf.Approximately(direction.y, 0.0f) &&
            Mathf.Approximately(direction.z, 0.0f) ||
            (double) Mathf.Abs(direction.y) > 0.0 && Mathf.Approximately(direction.x, 0.0f) &&
            Mathf.Approximately(direction.z, 0.0f) || (double) Mathf.Abs(direction.z) > 0.0 &&
            Mathf.Approximately(direction.x, 0.0f) && Mathf.Approximately(direction.y, 0.0f);

        private static Vector2 CalcDeltaAlongDirections(
            int id,
            Vector3 handlePos,
            Vector3 offset,
            Vector3 handleDir,
            Vector3 slideDir1,
            Vector3 slideDir2,
            float handleSize,
            Handles.CapFunction capFunction,
            Vector2 snap,
            bool drawHelper)
        {
            Vector3 position = handlePos + offset;
            Quaternion rotation = Quaternion.LookRotation(handleDir, slideDir1);
            Vector2 vector2_1 = new Vector2(0.0f, 0.0f);
            UnityEngine.Event current = UnityEngine.Event.current;
            switch (current.GetTypeForControl(id))
            {
                case UnityEngine.EventType.MouseDown:
                    if (HandleUtility.nearestControl == id && current.button == 0 && GUIUtility.hotControl == 0 &&
                        !current.alt)
                    {
                        Slider2D.s_CurrentMousePosition = current.mousePosition;
                        bool success = true;
                        Vector3 vector3 =
                            Handles.inverseMatrix.MultiplyPoint(Slider2D.GetMousePosition(handleDir, handlePos,
                                ref success));
                        if (success)
                        {
                            GUIUtility.hotControl = id;
                            Slider2D.s_StartPosition = handlePos;
                            Vector3 lhs = vector3 - handlePos;
                            Slider2D.s_StartPlaneOffset.x = Vector3.Dot(lhs, slideDir1);
                            Slider2D.s_StartPlaneOffset.y = Vector3.Dot(lhs, slideDir2);
                            current.Use();
                            EditorGUIUtility.SetWantsMouseJumping(1);
                        }

                        break;
                    }

                    break;
                case UnityEngine.EventType.MouseUp:
                    if (GUIUtility.hotControl == id && (current.button == 0 || current.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(0);
                        break;
                    }

                    break;
                case UnityEngine.EventType.MouseMove:
                case UnityEngine.EventType.Layout:
                    if (capFunction != null)
                    {
                        capFunction(id, position, rotation, handleSize, UnityEngine.EventType.Layout);
                        break;
                    }

                    HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(handlePos + offset, handleSize * 0.5f));
                    break;
                case UnityEngine.EventType.MouseDrag:
                    if (capFunction != null)
                        capFunction(id, position, rotation, handleSize, UnityEngine.EventType.Layout);
                    if (GUIUtility.hotControl == id)
                    {
                        Vector2 vector2_2 = current.mousePosition - Slider2D.s_CurrentMousePosition;
                        Slider2D.s_CurrentMousePosition += vector2_2;
                        bool success = true;
                        Vector3 point =
                            Handles.inverseMatrix.MultiplyPoint(Slider2D.GetMousePosition(handleDir, handlePos,
                                ref success));
                        if (success)
                        {
                            vector2_1.x =
                                HandleUtility.PointOnLineParameter(point, Slider2D.s_StartPosition, slideDir1);
                            vector2_1.y =
                                HandleUtility.PointOnLineParameter(point, Slider2D.s_StartPosition, slideDir2);
                            vector2_1 -= Slider2D.s_StartPlaneOffset;
                            vector2_1.x = Handles.SnapValue(vector2_1.x, snap.x);
                            vector2_1.y = Handles.SnapValue(vector2_1.y, snap.y);
                            GUI.changed = true;
                        }

                        current.Use();
                        break;
                    }

                    break;
                case UnityEngine.EventType.Repaint:
                    if (capFunction != null)
                    {
                        Color prevColor;
                        // Handles.SetupHandleColor(id, current, out prevColor, out float _);
                        capFunction(id, position, rotation, handleSize, UnityEngine.EventType.Repaint);
                        // Handles.color = prevColor;
                        if (drawHelper && GUIUtility.hotControl == id)
                        {
                            Vector3[] verts = new Vector3[4];
                            float num1 = handleSize * 10f;
                            verts[0] = position + (slideDir1 * num1 + slideDir2 * num1);
                            verts[1] = verts[0] - slideDir1 * num1 * 2f;
                            verts[2] = verts[1] - slideDir2 * num1 * 2f;
                            verts[3] = verts[2] + slideDir1 * num1 * 2f;
                            Handles.color = Color.white;
                            float num2 = 0.6f;
                            Handles.DrawSolidRectangleWithOutline(verts, new Color(1f, 1f, 1f, 0.05f),
                                new Color(num2, num2, num2, 0.4f));
                            // Handles.color = prevColor;
                            break;
                        }

                        break;
                    }

                    break;
            }

            return vector2_1;
        }

        private static Vector3 GetMousePosition(
            Vector3 handleDirection,
            Vector3 handlePosition,
            ref bool success)
        {
            if ((Object) Camera.current != (Object) null)
            {
                Plane plane = default;
                ref Plane local = ref plane;
                Matrix4x4 matrix = Handles.matrix;
                Vector3 inNormal = matrix.MultiplyVector(handleDirection);
                matrix = Handles.matrix;
                Vector3 inPoint = matrix.MultiplyPoint(handlePosition);
                local = new Plane(inNormal, inPoint);
                Ray worldRay = HandleUtility.GUIPointToWorldRay(Slider2D.s_CurrentMousePosition);
                float enter = 0.0f;
                success = plane.Raycast(worldRay, out enter);
                return worldRay.GetPoint(enter);
            }

            success = true;
            return (Vector3) Slider2D.s_CurrentMousePosition;
        }
    }
}