using System;
using System.Collections.Generic;
using UnityEngine;

namespace LIBII.Light2D
{
    [ExecuteInEditMode]
    public class Light2D : LightingMonoBehaviour
    {
#if UNITY_EDITOR
        public Gizmos gizmos;
#endif
        public enum LightType
        {
            Point = 0,
            FreeForm = 2,
        }

        public enum Rotation
        {
            Disabled,
            World,
            Local
        }

        public LightType lightType = LightType.Point;


        public Color color = new Color(.5f, .5f, .5f, 1);

        public float size = 5f;

        public float spotAngleInner = 360;
        public float spotAngleOuter = 360;


        public float lightStrength = 0;
        [Range(1.1f, 10f)] public float lightPower = 2;


        public Rotation applyRotation = Rotation.Disabled;

        public LightingSourceTextureSize textureSize = LightingSourceTextureSize.px2048;

        public MeshMode meshMode = new MeshMode();
        public BumpMap bumpMap = new BumpMap();


        public Sprite sprite;
        public bool spriteFlipX = false;
        public bool spriteFlipY = false;

        public LightTransform transform2D;

        public LightFreeForm freeForm;
        public float freeFormFalloff = 1;
        public float freeFormPoint = 1;
        public float freeFormFalloffStrength = 1;
        public FreeFormPoints freeFormPoints = new FreeFormPoints();


        public static List<Light2D> List = new List<Light2D>();
        private bool inScreen = false;
        public bool drawingEnabled = false;
        public bool drawingTranslucencyEnabled = false;
        private static Sprite defaultSprite = null;


        [System.Serializable]
        public class BumpMap
        {
            public float intensity = 1;
            public float depth = 1;
        }

        public void UpdateLoop()
        {
            transform2D.Update(this);


            transform2D.ForceUpdate();

            UpdateMeshMode();
        }

        public void LateUpdate()
        {
            UpdateLoop();
        }


        static public Sprite GetDefaultSprite()
        {
            if (defaultSprite == null || defaultSprite.texture == null)
            {
                defaultSprite = Resources.Load<Sprite>("Sprites/gfx_light");
            }

            return (defaultSprite);
        }

        public Sprite GetSprite()
        {
            if (sprite == null || sprite.texture == null)
            {
                sprite = GetDefaultSprite();
            }

            return (sprite);
        }

        public void ForceUpdate()
        {
            if (transform2D == null)
            {
                return;
            }

            transform2D.ForceUpdate();

            freeForm.ForceUpdate();

            UpdateMeshMode(true);
        }

        public static void ForceUpdateAll()
        {
            foreach (Light2D light in Light2D.List)
            {
                light.ForceUpdate();
            }
        }

        public void OnEnable()
        {
            List.Add(this);

            if (transform2D == null)
            {
                transform2D = new LightTransform();
            }

            if (freeForm == null)
            {
                freeForm = new LightFreeForm();
                freeForm.source = freeFormPoints;
            }

            ForceUpdate();
        }

        public void OnDisable()
        {
            List.Remove(this);

            Free();
        }

        public void Free()
        {
            inScreen = false;
        }


        // used to check if camera is used in the system

        public bool InCameras()
        {
            List<CameraTransform> lightingCameras = CameraTransform.List;

            Rect lightRect = transform2D.WorldRect;

            for (int i = 0; i < lightingCameras.Count; i++)
            {
                CameraTransform cameraTransform = lightingCameras[i];

                Camera camera = cameraTransform.Camera;

                if (camera == null)
                {
                    continue;
                }

                Rect cameraRect = cameraTransform.WorldRect();

                if (cameraRect.Overlaps(lightRect))
                {
                    return (true);
                }
            }

            return (false);
        }

        // to check if light is rendered for specific lightmap

        private Rect lightRect;
        private Rect cameraRect;
        public bool InCamera(Camera camera)
        {
            #if UNITY_EDITOR
                return true;
            #endif
            
            lightRect = transform2D.WorldRect;
            CameraTransform.Update();
            cameraRect = CameraTransform.GetWorldRect(camera);
            if (cameraRect.Overlaps(lightRect))
            {
                return (true);
            }

            return (false);
        }

        public void UpdateMeshMode(bool forceUpdate = false)
        {
            if (lightType == LightType.FreeForm)
            {
                freeForm.Update(this);
            }

            if (!isActiveAndEnabled)
            {
                return;
            }
            
            if (Application.isPlaying && !InCamera(Camera.main))
            {
                return;
            }

            LightingMeshRenderer lightingMesh = MeshRendererManager.Pull(this);

            if (lightingMesh != null)
            {
                lightingMesh.UpdateLight(this, meshMode, forceUpdate);
            }
        }
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (gizmos.drawGizmos != EditorDrawGizmos.Selected)
            {
                return;
            }

            Draw();
        }


        private void OnDrawGizmos()
        {
            /*UnityEngine.Gizmos.color = new Color(0, 1f, 1f);
            GizmosHelper.DrawRect(lightRect);
            if (Camera.main) GizmosHelper.DrawRect(cameraRect);*/
            
            if (gizmos.drawGizmos == EditorDrawGizmos.Disabled)
            {
                return;
            }

            if (gizmos.drawIcons == EditorIcons.Enabled)
            {
                UnityEngine.Gizmos.DrawIcon(transform.position, "light_v2", true);
            }

            if (gizmos.drawGizmos != EditorDrawGizmos.Always)
            {
                return;
            }

            Draw();
        }

        void Draw()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            UnityEngine.Gizmos.color = new Color(1f, 0.5f, 0.25f);

            if (applyRotation != Rotation.Disabled)
            {
                GizmosHelper.DrawCircle(transform.position, transform2D.rotation, 360, size); // spotAngle
            }
            else
            {
                GizmosHelper.DrawCircle(transform.position, 0, 360, size); // spotAngle
            }
        }
#endif
    }
}