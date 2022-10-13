using System;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using UnityEngine.Rendering;

namespace LBLibraryUnityShaderURP.Editor
{
    public class LitShader : BaseShaderGUI
    {
        private LitGUI.LitProperties litProperties;

        public static void SetMaterialKeywords(Material material)
        {
            if (material.HasProperty("_DetailAlbedoMap") && material.HasProperty("_DetailNormalMap") &&
                material.HasProperty("_DetailAlbedoMapScale"))
            {
                bool isScaled = material.GetFloat("_DetailAlbedoMapScale") != 1.0f;
                bool hasDetailMap = material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap");
                CoreUtils.SetKeyword(material, "_DETAIL_MULX2", !isScaled && hasDetailMap);
                CoreUtils.SetKeyword(material, "_DETAIL_SCALED", isScaled && hasDetailMap);
            }

            bool castShadows = true;
            if (material.HasProperty("_CastShadows"))
            {
                castShadows = (material.GetFloat("_CastShadows") != 0.0f);
            }

            material.SetShaderPassEnabled("ShadowCaster", castShadows);
        }
#if UNITY_2021_2_OR_NEWER
        public override void ValidateMaterial(Material material)
        {
            SetMaterialKeywords(material, LitGUI.SetMaterialKeywords, SetMaterialKeywords);
        }

#else
        public override void MaterialChanged(Material material)
        {
            SetMaterialKeywords(material, LitGUI.SetMaterialKeywords, SetMaterialKeywords);
        }
#endif
    }
}