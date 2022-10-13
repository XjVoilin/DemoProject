using UnityEditor;

namespace LIBII.Light2D
{
    [CustomEditor(typeof(Light2DRenderFeature))]
    public class Light2DRenderFeatureInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                    EditorGUILayout.PropertyField(iterator, true);
            }
            Light2DRenderFeature.Instance.light2DSettings.ManagerInternal =
                (ManagerInternal)EditorGUILayout.EnumPopup("Light 2D Mesh Proxy",
                    Light2DRenderFeature.Instance.light2DSettings.ManagerInternal);
            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}