using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;
using System;

namespace LIBII.Light2D
{
    public class GUIFoldout
    {
        static Dictionary<object, bool> dictionary = new Dictionary<object, bool>();

        static public bool GetValue(object Object)
        {
            bool value = false;

            if (!dictionary.TryGetValue(Object, out value))
            {
                dictionary.Add(Object, value);
            }

            return (value);
        }

        static public void SetValue(object Object, bool value)
        {
            bool resultVal;

            if (dictionary.TryGetValue(Object, out resultVal))
            {
                dictionary.Remove(Object);
                dictionary.Add(Object, value);
            }
        }

        static public bool Draw(string name, object Object)
        {
            bool value = EditorGUILayout.Foldout(GetValue(Object), name, true);

            SetValue(Object, value);

            return (value);
        }
    }

    public class GUIFoldoutHeader
    {
        static Dictionary<object, bool> dictionary = new Dictionary<object, bool>();

        static public bool GetValue(object Object)
        {
            bool value = false;

            if (!dictionary.TryGetValue(Object, out value))
            {
                dictionary.Add(Object, value);
            }

            return (value);
        }

        static public void SetValue(object Object, bool value)
        {
            bool resultVal;

            if (dictionary.TryGetValue(Object, out resultVal))
            {
                dictionary.Remove(Object);
                dictionary.Add(Object, value);
            }
        }

        static public bool Begin(string name, object Object)
        {
#if UNITY_2019_1_OR_NEWER
            bool value = EditorGUILayout.BeginFoldoutHeaderGroup(GetValue(Object), name);
#else
				bool value = EditorGUILayout.Foldout(GetValue(Object), name, true);
#endif

            SetValue(Object, value);

            return (value);
        }

        static public void End()
        {
#if UNITY_2019_1_OR_NEWER
            EditorGUILayout.EndFoldoutHeaderGroup();
#endif
        }
    }

    public class GUISortingLayer
    {
        static public string[] GetSortingLayerNames()
        {
            System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayersProperty =
                internalEditorUtilityType.GetProperty("sortingLayerNames",
                    BindingFlags.Static | BindingFlags.NonPublic);

            return (string[]) sortingLayersProperty.GetValue(null, new object[0]);
        }

        static public int[] GetSortingLayerUniqueIDs()
        {
            System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs",
                BindingFlags.Static | BindingFlags.NonPublic);

            return (int[]) sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
        }

        static public void Draw(SerializedObject serializedObject, LIBII.Light2D.SortingLayer sortingLayer,
            string serializationDepth = "")
        {
            SerializedProperty order = serializedObject.FindProperty(serializationDepth + "sortingLayer.Order");
            SerializedProperty name = serializedObject.FindProperty(serializationDepth + "sortingLayer.name");


            string[] sortingLayerNames = GetSortingLayerNames();
            int id = Array.IndexOf(sortingLayerNames, sortingLayer.Name);
            int newId = EditorGUILayout.Popup("Sorting Layer", id, sortingLayerNames);

            if (newId > -1 && newId < sortingLayerNames.Length)
            {
                string newName = sortingLayerNames[newId];

                if (newName != sortingLayer.Name)
                {
                    name.stringValue = newName;
                }
            }

            EditorGUILayout.PropertyField(order, new GUIContent("Order in Layer"));
        }

        static public void Draw(SortingLayer sortingLayer, bool drawFoldout)
        {
            if (drawFoldout)
            {
                bool value = GUIFoldout.Draw("Sorting Layer", sortingLayer);

                if (!value)
                {
                    return;
                }

                EditorGUI.indentLevel++;
            }

            string[] sortingLayerNames = GetSortingLayerNames();
            int id = Array.IndexOf(sortingLayerNames, sortingLayer.Name);
            int newId = EditorGUILayout.Popup("Sorting Layer", id, sortingLayerNames);

            if (newId > -1 && newId < sortingLayerNames.Length)
            {
                string newName = sortingLayerNames[newId];

                if (newName != sortingLayer.Name)
                {
                    sortingLayer.Name = newName;
                }
            }

            sortingLayer.Order = EditorGUILayout.IntField("Order in Layer", sortingLayer.Order);

            if (drawFoldout)
            {
                EditorGUI.indentLevel--;
            }
        }
    }

    public class GUIMeshMode
    {
        public static void Draw(SerializedObject serializedObject, MeshMode meshMode)
        {
            EditorGUILayout.LabelField("Renderer");


            EditorGUI.indentLevel++;
            SerializedProperty meshModeProxy = serializedObject.FindProperty("meshMode.proxy");

            SerializedProperty meshModeAlpha = serializedObject.FindProperty("meshMode.alpha");
            SerializedProperty meshModeShader = serializedObject.FindProperty("meshMode.shader");

            EditorGUILayout.PropertyField(meshModeProxy, new GUIContent("Proxy"));

            meshModeAlpha.floatValue = EditorGUILayout.Slider("Alpha", meshModeAlpha.floatValue, 0, 1);

            EditorGUILayout.PropertyField(meshModeShader, new GUIContent("Material"));

            if (meshModeShader.intValue == (int) MeshModeShader.Custom)
            {
                bool value2 = GUIFoldout.Draw("Materials", meshMode.materials);

                if (value2)
                {
                    EditorGUI.indentLevel++;

                    int count = meshMode.materials.Length;
                    count = EditorGUILayout.IntSlider("Material Count", count, 0, 10);

                    if (count != meshMode.materials.Length)
                    {
                        System.Array.Resize(ref meshMode.materials, count);
                    }

                    for (int id = 0; id < meshMode.materials.Length; id++)
                    {
                        Material material = meshMode.materials[id];

                        material = (Material) EditorGUILayout.ObjectField("Material", material, typeof(Material), true);

                        meshMode.materials[id] = material;
                    }

                    EditorGUI.indentLevel--;
                }
            }

            GUISortingLayer.Draw(serializedObject, meshMode.sortingLayer, "meshMode.");

            EditorGUI.indentLevel--;
        }
    }

    public class GUIBumpMapMode
    {
        static public void Draw(SerializedObject serializedObject, object obj)
        {
            // Serialized property
            bool value = GUIFoldout.Draw("Mask Bump Map", obj);

            if (!value)
            {
                return;
            }

            EditorGUI.indentLevel++;

            SerializedProperty bumpType = serializedObject.FindProperty("bumpMapMode.type");
            SerializedProperty bumpTextureType = serializedObject.FindProperty("bumpMapMode.textureType");
            SerializedProperty bumpTexture = serializedObject.FindProperty("bumpMapMode.texture");
            SerializedProperty bumpSprite = serializedObject.FindProperty("bumpMapMode.sprite");

            SerializedProperty invertX = serializedObject.FindProperty("bumpMapMode.invertX");
            SerializedProperty invertY = serializedObject.FindProperty("bumpMapMode.invertY");

            SerializedProperty depth = serializedObject.FindProperty("bumpMapMode.depth");

            SerializedProperty spriteRenderer = serializedObject.FindProperty("bumpMapMode.spriteRenderer");
            SpriteRenderer sr = (SpriteRenderer) spriteRenderer.objectReferenceValue;

            EditorGUILayout.PropertyField(bumpType, new GUIContent("Type"));
            EditorGUILayout.PropertyField(bumpTextureType, new GUIContent("Texture Type"));

            EditorGUILayout.PropertyField(invertX, new GUIContent("Invert X"));

            EditorGUILayout.PropertyField(invertY, new GUIContent("Invert Y"));

            EditorGUILayout.PropertyField(depth, new GUIContent("Depth"));


            EditorGUI.indentLevel--;
        }
    }
}