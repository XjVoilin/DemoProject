using UnityEngine;

namespace LIBII.Light2D
{
    public static class Lighting2D
    {
        public static Lighting2DMaterials materials = new Lighting2DMaterials();

        // disable
        public static bool disable => false;


        public static CoreAxis CoreAxis =>
            Light2DSettings.Instance == null ? CoreAxis.XY : Light2DSettings.Instance.coreAxis;
    }
}

//MyScriptableObjectClass asset = ScriptableObject.CreateInstance<MyScriptableObjectClass>();

//AssetDatabase.CreateAsset(asset, "Assets/NewScripableObject.asset");
//AssetDatabase.SaveAssets();

//EditorUtility.FocusProjectWindow();