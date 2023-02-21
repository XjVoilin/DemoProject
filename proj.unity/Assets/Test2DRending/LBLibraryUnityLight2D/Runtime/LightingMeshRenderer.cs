using System;
using System.Collections.Generic;
using UnityEngine;

namespace LIBII.Light2D
{
    [ExecuteInEditMode]
    public class LightingMeshRenderer : LightingMonoBehaviour
    {
        public static List<LightingMeshRenderer> list = new List<LightingMeshRenderer>();

        public bool free = true;
        public Light2D owner = null;

        public MeshRenderer meshRenderer = null;
        public MeshFilter meshFilter = null;

        private Material[] materials = new Material[1];

        public MeshModeShader meshModeShader = MeshModeShader.Additive;
        public Material[] meshModeMaterial = null;


        private string GetShaderName()
        {
            if (owner.lightType == Light2D.LightType.Point)
            {
                switch (meshModeShader)
                {
                    case MeshModeShader.Additive:
                        return "LIBII/Internal/Light/PointLight";
                    case MeshModeShader.Alpha:
                        return "LIBII/Internal/Light/PointLight_Alpha";
                    case MeshModeShader.Mask:
                        return "LIBII/Internal/Light/PointLight_Mask";
                }
            }

            switch (meshModeShader)
            {
                case MeshModeShader.Additive:
                    return "LIBII/Internal/MeshModeAdditive";

                case MeshModeShader.Alpha:
                    return "LIBII/Internal/MeshModeAlpha";
            }

            return string.Empty;
        }

        public Material[] GetMaterials()
        {
            if (materials == null)
            {
                materials = new Material[1];
            }

            if (materials.Length < 1)
            {
                materials = new Material[1];
            }

            var shaderName = GetShaderName();
            if (string.IsNullOrEmpty(shaderName))
            {
                materials = meshModeMaterial;
            }
            else
            {
                materials[0] = LightingMaterial.Load(shaderName).Get();
            }

            return (materials);
        }

        public static int GetCount()
        {
            return (list.Count);
        }

        public void OnDestroy()
        {
            list.Remove(this);
        }

        public static List<LightingMeshRenderer> List
        {
            get
            {
                if (list.Count == 0)
                {
                    list.AddRange(Resources.FindObjectsOfTypeAll<LightingMeshRenderer>());
                }

                return list;
            }
        }

        public void Initialize()
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();

            // Mesh System?
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            meshRenderer.allowOcclusionWhenDynamic = false;
        }

        public void Free()
        {
            owner = null;
            free = true;

            meshRenderer.enabled = false;

            if (meshRenderer.sharedMaterial != null)
            {
                meshRenderer.sharedMaterial.mainTexture = null;
            }
        }

        public void LateUpdate()
        {
            if (owner == null)
            {
                Free();
                return;
            }

            if (IsRendered())
            {
                meshRenderer.enabled = true;
            }
            else
            {
                Free();
                meshRenderer.enabled = false;
            }
        }

        public bool IsRendered()
        {
            Light2D light = (Light2D) owner;
            if (light)
            {
                return (light.isActiveAndEnabled
#if !UNITY_EDITOR
                        && light.InCameras()
#endif
                    );
            }

            return (false);
        }

        public void ClearMaterial()
        {
            materials = new Material[1];
        }

        public void UpdateLight(Light2D id, MeshMode meshMode, bool forceUpdate = false)
        {
            // Camera
            if (meshModeMaterial != meshMode.materials)
            {
                meshModeMaterial = meshMode.materials;

                ClearMaterial();
            }

            if (meshModeShader != meshMode.shader)
            {
                meshModeShader = meshMode.shader;

                ClearMaterial();
            }

            var transform1 = transform;
            var transform2 = id.transform;
            var localScale = transform2.localScale;

            if (id.lightType == Light2D.LightType.FreeForm)
            {
                transform1.position = transform2.position;
                transform1.localScale = localScale;
                transform1.rotation = transform2.rotation; // only if rotation enabled
            }
            else
            {
                transform1.position = transform2.position;
                transform1.rotation = transform2.rotation; // only if rotation enabled
                transform1.localScale =
                    new Vector3(id.size * localScale.x, id.size * localScale.y, 1);
            }


            if (meshRenderer != null)
            {
                Color lightColor = id.color;
                lightColor.a *= id.meshMode.alpha;

                var shaderName = GetShaderName();


                if (meshRenderer.sharedMaterial == null ||
                    meshRenderer.sharedMaterial.shader.name != shaderName ||
                    meshModeShader == MeshModeShader.Custom ||
                    materials[0] == null)
                {
                    materials = GetMaterials();
                    meshRenderer.sharedMaterial = materials[0];
                }

                if (meshRenderer.sharedMaterial != null)
                {
                    meshRenderer.sharedMaterial.color = lightColor;
                    meshRenderer.sharedMaterial.SetColor(Color1, lightColor);
                    meshRenderer.sharedMaterial.SetFloat(Inverted, 1);
                    meshRenderer.sharedMaterial.SetTexture(Sprite1, id.GetSprite().texture);


                    // material.mainTexture = light.Buffer.renderTexture.renderTexture;

                    if (id.lightType == Light2D.LightType.FreeForm)
                    {
                        meshRenderer.sharedMaterial.SetFloat(Strength, id.freeFormFalloffStrength);
                        meshRenderer.sharedMaterial.SetFloat(Point, id.freeFormPoint);
                    }
                    else
                    {
                        meshRenderer.sharedMaterial.SetFloat(Power, id.lightPower);

                        meshRenderer.sharedMaterial.SetFloat(Strength, id.lightStrength);
                        meshRenderer.sharedMaterial.SetFloat(Outer, id.spotAngleOuter - id.spotAngleInner);
                        meshRenderer.sharedMaterial.SetFloat(Inner, id.spotAngleInner);
                        meshRenderer.sharedMaterial.SetFloat(Rotation, id.transform2D.rotation * 0.0174533f);
                        // meshRenderer.sharedMaterial.SetTexture("_Lightmap", id.Buffer.renderTexture.renderTexture);        
                    }
                }


                id.meshMode.sortingLayer.ApplyToMeshRenderer(meshRenderer);

                meshRenderer.enabled = true;

                meshFilter.mesh = GetMeshLight(id, forceUpdate);
            }
        }

        // Light Source
        public Mesh getMeshLight = null;
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private static readonly int Inverted = Shader.PropertyToID("_Inverted");
        private static readonly int Sprite1 = Shader.PropertyToID("_MainTex");
        private static readonly int Strength = Shader.PropertyToID("_Strength");
        private static readonly int Outer = Shader.PropertyToID("_Outer");
        private static readonly int Inner = Shader.PropertyToID("_Inner");
        private static readonly int Rotation = Shader.PropertyToID("_Rotation");
        private static readonly int Point = Shader.PropertyToID("_Point");
        private static readonly int Power = Shader.PropertyToID("_Power");

        public Mesh GetMeshLight(Light2D light, bool forceUpdate = false)
        {
            if (getMeshLight == null || forceUpdate || light.lightType == Light2D.LightType.FreeForm
                || (getMeshLight.vertexCount > 4 && light.lightType == Light2D.LightType.Point))
            {
                switch (light.lightType)
                {
                    case Light2D.LightType.Point:
                        Mesh mesh = new Mesh();

                        mesh.vertices = new Vector3[]
                            {new Vector3(-1, -1), new Vector3(1, -1), new Vector3(1, 1), new Vector3(-1, 1)};
                        mesh.triangles = new int[] {2, 1, 0, 0, 3, 2};
                        mesh.uv = new Vector2[]
                            {new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)};

                        getMeshLight = mesh;
                        break;
                    case Light2D.LightType.FreeForm:
                        getMeshLight = light.freeForm.GetMesh();
                        break;
                }
            }

            return (getMeshLight);
        }
    }
}