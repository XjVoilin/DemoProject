using UnityEngine;

namespace LIBII.Light2D
{
    [ExecuteInEditMode]
    public class MeshRendererManager
    {
        // management
        public static LightingMeshRenderer AddBuffer(Light2D source)
        {
            if (!Light2DRenderFeature.Instance || Light2DRenderFeature.Instance.light2DSettings == null)
                return null;

            GameObject meshRenderer =
                new GameObject("Mesh Renderer (Id :" + (LightingMeshRenderer.GetCount() + 1) + ")");
            meshRenderer.transform.parent = LightingManager2D.Get().transform;
            if (Light2DRenderFeature.Instance.light2DSettings.ManagerInternal == ManagerInternal.HideInHierarchy)
            {
                meshRenderer.hideFlags =
                    HideFlags.HideInHierarchy | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            }
            else
            {
                meshRenderer.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            }

            LightingMeshRenderer lightBuffer2D = meshRenderer.AddComponent<LightingMeshRenderer>();
            LightingMeshRenderer.List.Add(lightBuffer2D);

            lightBuffer2D.Initialize();

            lightBuffer2D.owner = source;
            lightBuffer2D.free = false;

            return (lightBuffer2D);
        }

        public static LightingMeshRenderer Pull(Light2D source)
        {
            foreach (LightingMeshRenderer id in LightingMeshRenderer.List)
            {
                if (id && id.owner == source && id.gameObject)
                {
                    id.gameObject.SetActive(true);
                    source.meshMode.proxy = id;

                    return (id);
                }
            }

            foreach (LightingMeshRenderer id in LightingMeshRenderer.List)
            {
                if (id && id.free)
                {
                    id.free = false;
                    id.owner = source;
                    source.meshMode.proxy = id;
                    id.gameObject.SetActive(true);

                    return (id);
                }
            }

            return (AddBuffer(source));
        }
    }
}