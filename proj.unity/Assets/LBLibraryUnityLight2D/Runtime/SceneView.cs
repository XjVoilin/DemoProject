using UnityEngine;

//#if UNITY_EDITOR
//	using UnityEditor;
//#endif

namespace LIBII.Light2D
{
    public class SceneView
    {
        public SceneView()
        {
        }

#if UNITY_EDITOR
        private void OnSceneView(UnityEditor.SceneView sceneView)
        {
            if (Application.isPlaying) return;

            LightingManager2D manager = LightingManager2D.Get();

            if (!IsSceneViewActive())
            {
                return;
            }

            Main.InternalUpdate();

            Main.Render();
        }
#endif

        public void OnDisable()
        {
#if UNITY_EDITOR

#if UNITY_2019_1_OR_NEWER

            UnityEditor.SceneView.beforeSceneGui -= OnSceneView;
            //SceneView.duringSceneGui -= OnSceneView;

#else
					UnityEditor.SceneView.onSceneGUIDelegate -= OnSceneView;

#endif

#endif
        }

        public void OnEnable()
        {
#if UNITY_EDITOR

#if UNITY_2019_1_OR_NEWER
            UnityEditor.SceneView.beforeSceneGui += OnSceneView;

#else
            UnityEditor.SceneView.onSceneGUIDelegate += OnSceneView;

#endif
#endif
        }

        public bool IsSceneViewActive() // overlay
        {
            LightingManager2D manager = LightingManager2D.Get();

            for (int i = 0; i < manager.cameras.Length; i++)
            {
                CameraSettings cameraSetting = manager.cameras.Get(i);
            }

            return (true);
        }
    }
}