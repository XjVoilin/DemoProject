using UnityEngine;

namespace LIBII.Light2D
{
    public static class Main
    {
        public static void InternalUpdate()
        {
            UpdateCameras();

            CameraTransform.Update();

            // UpdateMaterials();
        }

        public static void Render()
        {
            if (Lighting2D.disable)
            {
                return;
            }

            LightingCameras cameras = LightingManager2D.Get().cameras;

            if (cameras.Length < 1)
            {
                return;
            }

            UpdateLoop();
        }

        private static void UpdateLoop()
        {
            // lights
            if (Light2D.List.Count > 0)
            {
                for (int id = 0; id < Light2D.List.Count; id++)
                {
                    Light2D.List[id].UpdateLoop();
                }
            }
        }

        public static void UpdateCameras()
        {
            // should reset materials


            LightingCameras cameras = LightingManager2D.Get().cameras;

            for (int i = 0; i < cameras.Length; i++)
            {
                CameraSettings cameraSetting = cameras.Get(i);
            }
        }

     
        public static void UpdateMaterials()
        {
            if (Lighting2D.materials.Initialize())
            {
                Light2D.ForceUpdateAll();
            }
        }
    }
}