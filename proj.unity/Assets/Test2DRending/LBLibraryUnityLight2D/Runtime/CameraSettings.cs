using UnityEngine;

//#if UNITY_EDITOR

//    using UnityEditor;

//#endif

namespace LIBII.Light2D
{
    [System.Serializable]
    public struct CameraSettings
    {
        public enum CameraType
        {
            MainCamera,
            Custom,
            SceneView
        };

        public static int initCount = 0;

        public int id;


     

        public CameraType cameraType;
        public Camera customCamera;

        public string GetTypeName()
        {
            switch (cameraType)
            {
                case CameraType.SceneView:

                    return ("Scene View");

                case CameraType.MainCamera:

                    return ("Main Camera Tag");

                case CameraType.Custom:

                    return ("Custom");

                default:

                    return ("Unknown");
            }
        }

   
        public CameraSettings(int id)
        {
            this.id = id;

            cameraType = CameraType.MainCamera;

            customCamera = null;


            initCount++;
        }

        public Camera GetCamera()
        {
            Camera camera = null;

            switch (cameraType)
            {
                case CameraType.MainCamera:

                    camera = Camera.main;

                    if (camera != null)
                    {
                        if (!camera.orthographic)
                        {
                            return (null);
                        }
                    }

                    return (Camera.main);

                case CameraType.Custom:

                    camera = customCamera;

                    if (camera != null)
                    {
                        if (!camera.orthographic)
                        {
                            return (null);
                        }
                    }

                    return (customCamera);


                case CameraType.SceneView:

#if UNITY_EDITOR

                    UnityEditor.SceneView sceneView = UnityEditor.SceneView.lastActiveSceneView;

                    if (sceneView != null)
                    {
                        camera = sceneView.camera; // .GetComponent<Camera>();

#if UNITY_2019_1_OR_NEWER

                        if (!UnityEditor.SceneView.lastActiveSceneView.sceneLighting)
                        {
                            camera = null;
                        }

#else
								if (!UnityEditor.SceneView.lastActiveSceneView.m_SceneLighting)
								{
									camera = null;
								}

#endif
                    }

                    if (camera != null && !camera.orthographic)
                    {
                        camera = null;
                    }

                    if (camera != null)
                    {
                        if (!camera.orthographic)
                        {
                            return (null);
                        }
                    }

                    return (camera);

#else
						return(null);

#endif
            }

            return (null);
        }

        /*
        public bool Equals(CameraSettings obj) {
            return this.bufferID == obj.bufferID && this.customCamera == obj.customCamera && this.cameraType == obj.cameraType;
        }*/

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }
    }
}