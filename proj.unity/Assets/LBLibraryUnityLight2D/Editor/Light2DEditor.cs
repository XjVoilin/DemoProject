using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LIBII.Light2D
{
    [CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(Light2D))]
    public class Light2DEditor : UnityEditor.Editor
    {
        private Light2D light2D;

        private SerializedProperty lightType;


        private SerializedProperty color;

        private SerializedProperty size;
        private SerializedProperty spotAngleInner;
        private SerializedProperty spotAngleOuter;


        private SerializedProperty textureSize;

        private SerializedProperty lightSprite;
        private SerializedProperty spriteFlipX;
        private SerializedProperty spriteFlipY;
        private SerializedProperty sprite;

        private SerializedProperty freeFormPoints;
        private SerializedProperty freeFormFalloff;
        private SerializedProperty freeFormPoint;
        private SerializedProperty freeFormFalloffStrength;


        private bool foldoutSprite = false;
        private bool foldoutBumpMap = false;
        private bool foldoutFreeForm = false;
        private bool foldoutTranslucency = false;

        private void InitProperties()
        {
            lightType = serializedObject.FindProperty("lightType");


            color = serializedObject.FindProperty("color");

            size = serializedObject.FindProperty("size");
            spotAngleInner = serializedObject.FindProperty("spotAngleInner");
            spotAngleOuter = serializedObject.FindProperty("spotAngleOuter");


            textureSize = serializedObject.FindProperty("textureSize");

            lightSprite = serializedObject.FindProperty("lightSprite");
            spriteFlipX = serializedObject.FindProperty("spriteFlipX");
            spriteFlipY = serializedObject.FindProperty("spriteFlipY");
            sprite = serializedObject.FindProperty("sprite");


            freeFormPoints = serializedObject.FindProperty("freeFormPoints.points");

            freeFormFalloff = serializedObject.FindProperty("freeFormFalloff");

            freeFormPoint = serializedObject.FindProperty("freeFormPoint");

            freeFormFalloffStrength = serializedObject.FindProperty("freeFormFalloffStrength");
        }

        private void OnEnable()
        {
            light2D = target as Light2D;

            InitProperties();

            Undo.undoRedoPerformed += RefreshAll;
        }

        internal void OnDisable()
        {
            Undo.undoRedoPerformed -= RefreshAll;
        }

        void RefreshAll()
        {
            Light2D.ForceUpdateAll();
        }

        private void DrawPoints(List<Vector2> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = GetPointWorldPosition(i);
                Vector3 nextPoint = GetPointWorldPosition((i + 1) % points.Count);

                Handles.DrawLine(point, nextPoint, 2);
            }
        }

        public Camera GetSceneCamera()
        {
            UnityEditor.SceneView sceneView = UnityEditor.SceneView.lastActiveSceneView;

            Camera camera = null;

            if (sceneView != null)
            {
                camera = sceneView.camera;

                if (!camera.orthographic)
                {
                    camera = null;
                }
            }

            return (camera);
        }


        private bool OnScene_FreeForm()
        {
            Camera camera = GetSceneCamera();

            if (camera == null)
            {
                return (false);
            }

            bool changed = true;

            float cameraSize = camera.orthographicSize;

            Handles.color = new Color(1, 0.4f, 0);

            List<Vector2> points = light2D.freeFormPoints.points;

            bool intersect = false;


            var controlIds = new int[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = GetPointWorldPosition(i);
                Vector3 nextPoint = GetPointWorldPosition((i + 1) % points.Count);


                controlIds[i] = GUIUtility.GetControlID($"PointHandle_{i}".GetHashCode(), FocusType.Passive);

                Handles.DrawLine(point, nextPoint);

                Vector3 result = Handles.FreeMoveHandle(controlIds[i],
                    point, Quaternion.identity, 0.05f * cameraSize,
                    Vector2.zero,
                    Handles.CylinderHandleCap);

                if (point != result)
                {
                    result = light2D.transform.InverseTransformPoint(result);

                    List<Vector2> cPoints = new List<Vector2>(points);

                    cPoints[i] = result;

                    if (Math2D.PolygonIntersectItself(cPoints))
                    {
                        intersect = true;
                    }
                    else
                    {
                        points[i] = result;

                        changed = true;
                    }
                }
            }


            if (UnityEngine.Event.current.control
                && UnityEngine.Event.current.type == EventType.MouseDown)
            {
                GUIUtility.hotControl = HandleUtility.nearestControl;

                for (int i = 0; i < controlIds.Length; i++)
                {
                    if (GUIUtility.hotControl == controlIds[i])
                    {
                        RemovePointAt(i);
                    }
                }
            }

            var ret = GetMouseClosestEdgeDistance(camera);
            // Debug.Log("====>" + ret.Item1);
            ret.Item2[0].z = light2D.transform.position.z;
            var pos = FindClosestPointOnEdge(ret.Item1, ret.Item2[0]);

            if (Vector2.Distance(ret.Item2[0], pos) < .5f && !controlIds.Contains(HandleUtility.nearestControl))
            {
                var from = ret.Item2[1];
                var to = ret.Item2[0];
                pos = Handles.Slider2D(pos,
                    to - from,
                    Vector3.up,
                    Vector3.right,
                    0.05f * camera.orthographicSize,
                    Handles.CylinderHandleCap,
                    Vector2.zero);

                if (UnityEngine.Event.current.type == EventType.MouseDown
                    && UnityEngine.Event.current.shift)
                {
                    Debug.Log($"Add point:{pos}");
                    InsertPointAt(NextIndex(ret.Item1, GetPointsCount()), light2D.transform.InverseTransformPoint(pos));
                }
            }

            if (!intersect)
            {
                DrawPoints(points);
            }

            return (changed);
        }

        private int m_MouseClosestEdge = -1;
        private float m_MouseClosestEdgeDist = float.MaxValue;
        private Vector3[][] m_EdgePoints;

        void PrepareEdgePointList()
        {
            {
                var total = this.GetPointsCount();
                int loopCount = total;
                m_EdgePoints = new Vector3[loopCount][];
                int index = mod(total - 1, loopCount);
                for (int loop = mod(index + 1, total); loop < total; index = loop, ++loop)
                {
                    var position0 = this.GetPointWorldPosition(index);
                    var position1 = this.GetPointWorldPosition(loop);
                    m_EdgePoints[index] = new Vector3[] {position0, position1};
                }
            }
        }

        private (int, Vector3[]) GetMouseClosestEdgeDistance(Camera camera)
        {
            var mouseWorldPosition = ScreenToWorld(camera, UnityEngine.Event.current.mousePosition);
            Handles.DrawWireCube(mouseWorldPosition + Vector3.forward * 10, Vector3.one * .4f);
            var total = this.GetPointsCount();
            var p = new Vector3[2];
            m_MouseClosestEdge = -1;
            if (total > 0)
            {
                PrepareEdgePointList();

                m_MouseClosestEdgeDist = float.MaxValue;

                int loopCount = total;
                for (int loop = 0; loop < loopCount; loop++)
                {
                    var q = m_EdgePoints[loop];
                    var dist = DistancePointEdge(mouseWorldPosition, q);
                    if (dist < m_MouseClosestEdgeDist)
                    {
                        m_MouseClosestEdge = loop;
                        m_MouseClosestEdgeDist = dist;
                        p = q;
                    }
                }
            }

            return (m_MouseClosestEdge, new Vector3[3] {mouseWorldPosition, p[0], p[1]});
        }

        float DistancePointEdge(Vector3 point, Vector3[] edge)
        {
            return HandleUtility.DistancePointLine(point, edge[0], edge[1]);
        }


        public Func<int, Vector3> GetPointLTangent = i => Vector3.zero;
        public Action<int, Vector3> SetPointLTangent = (i, p) => { };
        public Func<int, Vector3> GetPointRTangent = i => Vector3.zero;
        public Action<int, Vector3> SetPointRTangent = (i, p) => { };

        private static Vector3 GetPoint(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent,
            Vector3 endTangent, float t)
        {
            t = Mathf.Clamp01(t);
            float a = 1f - t;
            return a * a * a * startPosition + 3f * a * a * t * (startPosition + startTangent) +
                   3f * a * t * t * (endPosition + endTangent) + t * t * t * endPosition;
        }

        private static int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        private static int NextIndex(int index, int total)
        {
            return mod(index + 1, total);
        }

        private int GetPointsCount()
        {
            return light2D.freeFormPoints.points.Count;
        }

        private Vector3 GetPointWorldPosition(int index)
        {
            return light2D.transform.TransformPoint(light2D.freeFormPoints.points[index]);
        }


        public Vector3 FindClosestPointOnEdge(int edgeIndex, Vector3 position)
        {
            var from = GetPointWorldPosition(edgeIndex);
            var to = GetPointWorldPosition(NextIndex(edgeIndex, GetPointsCount()));

            var dir = (to - from).normalized;

            return Vector3.Dot(position - from, dir) * dir + from;
        }

        private int m_NewPointIndex = -1;
        private static Color handleOutlineColor { get; set; }
        private static Color handleFillColor { get; set; }

        private static readonly Color[] k_OutlineColor = new[]
        {
            Color.gray,
            Color.white,
            new Color(34f / 255f, 171f / 255f, 1f), // #22abff
            Color.white
        };

        static readonly Color[] k_FillColor = new[]
        {
            Color.white,
            new Color(131f / 255f, 220f / 255f, 1f), // #83dcff
            new Color(34f / 255f, 171f / 255f, 1f), // #22abff
            new Color(34f / 255f, 171f / 255f, 1f) // #22abff
        };

        enum ColorEnum
        {
            EUnselected,
            EUnselectedHovered,
            ESelected,
            ESelectedHovered
        }


        private Vector3 ScreenToWorld(Camera camera, Vector2 point)
        {
            float ppp = EditorGUIUtility.pixelsPerPoint;
            point.y = camera.pixelHeight - point.y * ppp;
            point.x *= ppp;
            return camera.ScreenToWorldPoint(point);
            // return Handles.inverseMatrix.MultiplyPoint(point);
        }

        public void InsertPointAt(int pointIndex, Vector3 position)
        {
            RecordUndo();
            Insert(pointIndex, position);
        }

        public void Insert(int index, Vector2 point)
        {
            light2D.freeFormPoints.points.Insert(index, point);
        }

        public void RemovePointAt(int pointIndex)
        {
            RecordUndo();
            light2D.freeFormPoints.points.RemoveAt(pointIndex);
        }

        private void RecordUndo()
        {
            Undo.RegisterCompleteObjectUndo(target, "Outline changed");
        }

        private bool OnScene_Point()
        {
            Camera camera = GetSceneCamera();

            if (camera == null)
            {
                return (false);
            }

            bool changed = false;

            Handles.color = new Color(1, 0.4f, 0);

            float cameraSize = camera.orthographicSize;

            Vector3 point = light2D.transform.position;

            float rotation = (light2D.transform.localRotation.eulerAngles.z + 90) * Mathf.Deg2Rad;

            point.x += Mathf.Cos(rotation) * light2D.size;
            point.y += Mathf.Sin(rotation) * light2D.size;

            Vector3 result = Handles.FreeMoveHandle(point, Quaternion.identity, 0.05f * cameraSize, Vector2.zero,
                Handles.CylinderHandleCap);

            float moveDistance = Vector2.Distance(point, result);

            if (moveDistance > 0)
            {
                float newSize = Vector2.Distance(result, light2D.transform2D.position);

                light2D.size = newSize;

                changed = true;
            }

            float originAngle = 90f +
                                (int) (Mathf.Atan2(light2D.transform.position.y - point.y,
                                    light2D.transform.position.x - point.x) * Mathf.Rad2Deg);

            float rotateAngle = 90f +
                                (int) (Mathf.Atan2(light2D.transform.position.y - result.y,
                                    light2D.transform.position.x - result.x) * Mathf.Rad2Deg);

            rotateAngle = Math2D.NormalizeRotation(rotateAngle);
            originAngle = Math2D.NormalizeRotation(originAngle);

            if (Mathf.Abs(rotateAngle - originAngle) > 0.001f)
            {
                Quaternion QRotation = light2D.transform.localRotation;

                Vector3 vRotation = QRotation.eulerAngles;

                vRotation.z = rotateAngle;

                light2D.transform.localRotation = Quaternion.Euler(vRotation);

                changed = true;
            }

            Vector3 innerPoint = light2D.transform.position;

            float innerValue = light2D.spotAngleInner / 180 - 1;

            innerPoint.x -= Mathf.Cos(rotation) * light2D.size * innerValue;

            innerPoint.y -= Mathf.Sin(rotation) * light2D.size * innerValue;

            Handles.color = new Color(1f, 0.5f, 0.5f);

            Vector3 innerHandle = Handles.FreeMoveHandle(innerPoint, Quaternion.identity, 0.05f * cameraSize,
                Vector2.zero, Handles.CylinderHandleCap);

            if (Vector2.Distance(innerHandle, innerPoint) > 0.001f)
            {
                float nextInnerAngle = Vector2.Distance(innerHandle, point) / light2D.size;

                nextInnerAngle = Math2D.Range(nextInnerAngle * 180, 0, 360);

                if (Vector2.Distance(innerHandle, light2D.transform.position) > light2D.size)
                {
                    float a = Vector2.Distance(innerHandle, light2D.transform.position);
                    float b = Vector2.Distance(innerHandle, point);

                    nextInnerAngle = (b < a) ? 0 : 360;
                }

                float nextOuterAngle = nextInnerAngle + (light2D.spotAngleOuter - light2D.spotAngleInner);
                nextOuterAngle = Math2D.Range(Mathf.Max(nextInnerAngle, nextOuterAngle), 0, 360);

                light2D.spotAngleOuter = nextOuterAngle;

                light2D.spotAngleInner = nextInnerAngle;

                changed = true;
            }

            Handles.color = new Color(0.5f, 0.5f, 1f);

            if (light2D.spotAngleInner < 360)
            {
                Vector3 outerPointLeft = light2D.transform.position;

                float outerValue = (light2D.spotAngleOuter) * Mathf.Deg2Rad * 0.5f;

                outerPointLeft.x += Mathf.Cos(rotation + outerValue) * light2D.size;

                outerPointLeft.y += Mathf.Sin(rotation + outerValue) * light2D.size;

                Vector3 outerHandleLeft = Handles.FreeMoveHandle(outerPointLeft, Quaternion.identity,
                    0.05f * cameraSize, Vector2.zero, Handles.CylinderHandleCap);

                float transformRotation = light2D.transform.rotation.eulerAngles.z;

                if (Vector2.Distance(outerPointLeft, outerHandleLeft) > 0.001f)
                {
                    originAngle = 90f + (int) (Mathf.Atan2(light2D.transform.position.y - outerPointLeft.y,
                        light2D.transform.position.x - outerPointLeft.x) * Mathf.Rad2Deg);
                    originAngle -= transformRotation;
                    originAngle = Math2D.NormalizeRotation(originAngle);

                    rotateAngle = 90f + (int) (Mathf.Atan2(light2D.transform.position.y - outerHandleLeft.y,
                        light2D.transform.position.x - outerHandleLeft.x) * Mathf.Rad2Deg);
                    rotateAngle -= transformRotation;
                    rotateAngle = Math2D.NormalizeRotation(rotateAngle);

                    light2D.spotAngleOuter = rotateAngle * 2f;
                    light2D.spotAngleOuter =
                        Math2D.Range(Mathf.Max(light2D.spotAngleInner, light2D.spotAngleOuter), 0, 360);

                    changed = true;
                }

                Vector3 outerPointRight = light2D.transform.position;

                outerPointRight.x += Mathf.Cos(rotation - outerValue) * light2D.size;

                outerPointRight.y += Mathf.Sin(rotation - outerValue) * light2D.size;

                Vector3 outerHandleRight = Handles.FreeMoveHandle(outerPointRight, Quaternion.identity,
                    0.05f * cameraSize, Vector2.zero, Handles.CylinderHandleCap);

                if (Vector2.Distance(outerPointRight, outerHandleRight) > 0.01f)
                {
                    originAngle = -90f - (int) (Mathf.Atan2(light2D.transform.position.y - outerPointRight.y,
                        light2D.transform.position.x - outerPointRight.x) * Mathf.Rad2Deg);
                    originAngle += transformRotation;
                    originAngle = Math2D.NormalizeRotation(originAngle);

                    rotateAngle = -90f - (int) (Mathf.Atan2(light2D.transform.position.y - outerHandleRight.y,
                        light2D.transform.position.x - outerHandleRight.x) * Mathf.Rad2Deg);
                    rotateAngle += transformRotation;
                    rotateAngle = Math2D.NormalizeRotation(rotateAngle);

                    light2D.spotAngleOuter = rotateAngle * 2f;

                    light2D.spotAngleOuter =
                        Math2D.Range(Mathf.Max(light2D.spotAngleInner, light2D.spotAngleOuter), 0, 360);

                    changed = true;
                }
            }

            return (changed);
        }

        private void OnSceneGUI()
        {
            if (light2D == null)
            {
                return;
            }

            bool changed = false;

            switch (light2D.lightType)
            {
                case Light2D.LightType.FreeForm:
                    changed = OnScene_FreeForm();
                    break;

                case Light2D.LightType.Point:
                    changed = OnScene_Point();
                    break;
            }

            if (changed)
            {
                light2D.ForceUpdate();

                if (!EditorApplication.isPlaying)
                {
                    EditorUtility.SetDirty(target);

                    EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (light2D == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(lightType, new GUIContent("Type"));

            EditorGUILayout.Space();


            light2D.applyRotation = (Light2D.Rotation) EditorGUILayout.EnumPopup("Rotation", light2D.applyRotation);

            EditorGUILayout.Space();

            Color colorValue = EditorGUILayout.ColorField(new GUIContent("Color"), color.colorValue, true, true, true);

            colorValue.a = EditorGUILayout.Slider("Alpha", colorValue.a, 0, 1);

            color.colorValue = colorValue;

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(light2D.lightType == Light2D.LightType.FreeForm);

            size.floatValue =
                EditorGUILayout.Slider("Size", size.floatValue, 0.1f, Light2DSettings.Instance.maxLightSize);

            EditorGUI.EndDisabledGroup();

            // LIGHT PRESET PROPERTIES ///////////////////////////////////////

            float inner = spotAngleInner.floatValue;
            float outer = spotAngleOuter.floatValue;

            double roundInner = System.Math.Round(inner, 2);
            double roundOuter = System.Math.Round(outer, 2);

            switch (light2D.lightType)
            {
                case Light2D.LightType.Point:

                    EditorGUILayout.MinMaxSlider("Spot Angle (" + roundInner + ", " + roundOuter + ")", ref inner,
                        ref outer, 0f, 360f);

                    spotAngleInner.floatValue = inner;
                    spotAngleOuter.floatValue = outer;

                    light2D.lightStrength = EditorGUILayout.Slider("Falloff", light2D.lightStrength, 0, 1);
                    light2D.lightPower = EditorGUILayout.Slider("Power", light2D.lightPower, 1.1f, 10);
                    break;
            }


            //////////////////////////////////////////////////////////////////////////////

            EditorGUILayout.Space();

            switch (light2D.lightType)
            {
                case Light2D.LightType.FreeForm:

                    foldoutFreeForm = EditorGUILayout.Foldout(foldoutFreeForm, "Free Form", true);

                    if (foldoutFreeForm)
                    {
                        EditorGUI.indentLevel++;

                        freeFormPoint.floatValue = EditorGUILayout.Slider("Point", freeFormPoint.floatValue, 0, 1);

                        EditorGUILayout.PropertyField(freeFormFalloff, new GUIContent("Falloff"));

                        freeFormFalloffStrength.floatValue = EditorGUILayout.Slider("Falloff Strength",
                            freeFormFalloffStrength.floatValue, 0, 1);

                        freeFormFalloff.floatValue = Mathf.Max(freeFormFalloff.floatValue, 0);

                        EditorGUILayout.PropertyField(freeFormPoints, new GUIContent("Points"));

                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Space();

                    break;
            }


            EditorGUILayout.Space();

            GUIMeshMode.Draw(serializedObject, light2D.meshMode);


            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                foreach (UnityEngine.Object target in targets)
                {
                    if (target == null)
                    {
                        continue;
                    }

                    Light2D light2D = target as Light2D;

                    if (light2D == null)
                    {
                        continue;
                    }

                    light2D.ForceUpdate();

                    if (!EditorApplication.isPlaying)
                    {
                        EditorUtility.SetDirty(target);
                    }
                }

                if (!EditorApplication.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                }
            }
        }
    }
}