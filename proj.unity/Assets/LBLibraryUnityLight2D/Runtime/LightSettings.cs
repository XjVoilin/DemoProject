using System;
using UnityEngine;

namespace LIBII.Light2D
{
    [Serializable]
    public class Light2DSettings
    {
        public static Light2DSettings Instance;

        public CoreAxis coreAxis;

        public Gizmos gizmos;

        public int maxLightSize = 100;

        private ManagerInternal managerInternal = ManagerInternal.HideInHierarchy;

        public ManagerInternal ManagerInternal
        {
            get => managerInternal;
            set
            {
                if (managerInternal != value)
                {
                    managerInternal = value;
                    if (managerInternal == ManagerInternal.HideInHierarchy)
                    {
                        foreach (var light in LightingMeshRenderer.List)
                        {
                            light.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInBuild |
                                                         HideFlags.DontSaveInEditor;
                        }
                    }
                    else
                    {
                        foreach (var light in LightingMeshRenderer.List)
                        {
                            light.gameObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                        }
                    }
                }
            }
        }
    }

    public enum ManagerInternal
    {
        HideInHierarchy,
        ShowInHierarchy
    }

    [System.Serializable]
    public class MeshMode
    {
        [Range(0, 1)] public float alpha = 0.5f;

        public MeshModeShader shader = MeshModeShader.Additive;
        public Material[] materials = new Material[1];

        public SortingLayer sortingLayer = new SortingLayer();
        public LightingMeshRenderer proxy;
    }

    public enum MeshModeShader
    {
        Additive,
        Alpha,
        Custom,
        Mask
    }

    [System.Serializable]
    public class SortingLayer
    {
        [SerializeField] private string name = "Default";

        public string Name
        {
            get
            {
                if (name.Length < 1)
                {
                    name = "Default";
                }

                return (name);
            }

            set => name = value;
        }

        public int Order = 0;

        public void ApplyToMeshRenderer(MeshRenderer meshRenderer)
        {
            if (meshRenderer == null)
            {
                return;
            }

            if (meshRenderer.sortingLayerName != Name)
            {
                meshRenderer.sortingLayerName = Name;
            }

            if (meshRenderer.sortingOrder != Order)
            {
                meshRenderer.sortingOrder = Order;
            }
        }
    }
}