using UnityEngine;

namespace LIBII.Light2D
{
    [ExecuteInEditMode]
    public class LightingManager2D : LightingMonoBehaviour
    {
        private static LightingManager2D instance;
        [SerializeField]
        public LightingCameras cameras = new LightingCameras();


        // editor foldouts (avoid reseting after compiling script)
        public bool[] foldout_cameras = new bool[10];

        public bool[,] foldout_lightmapPresets = new bool[10, 10];
        public bool[,] foldout_lightmapMaterials = new bool[10, 10];

        // Sets Lighting Main Profile Settings for Lighting2D at the start of the scene
        private static bool initialized = false;
        private SceneView sceneView = new SceneView();

        public static void ForceUpdate()
        {
        }

        public static LightingManager2D Get()
        {
            if (instance != null)
            {
                return (instance);
            }

            foreach (LightingManager2D manager in UnityEngine.Object.FindObjectsOfType(typeof(LightingManager2D)))
            {
                instance = manager;

                return (instance);
            }

            // create new light manager
            GameObject gameObject = new GameObject("Lighting Manager 2D");

            instance = gameObject.AddComponent<LightingManager2D>();

            instance.transform.position = Vector3.zero;


            return (instance);
        }

        public void Awake()
        {
            CameraTransform.List.Clear();

            if (instance == null)
            {
                instance = this;
            }

            if (instance != null && instance != this)
            {
                Debug.LogWarning(
                    "Smart Lighting2D: Lighting Manager duplicate was found, new instance destroyed.",
                    gameObject);

                foreach (LightingManager2D manager in UnityEngine.Object.FindObjectsOfType(
                    typeof(LightingManager2D)))
                {
                    if (manager != instance)
                    {
                        manager.DestroySelf();
                    }
                }
            }

            LightingManager2D.initialized = false;


            if (Application.isPlaying)
            {
                DontDestroyOnLoad(instance.gameObject);
            }
        }

        // use only late update?
        private void Update()
        {
            if (Lighting2D.disable)
            {
                return;
            }

            ForceUpdate(); // for late update method?


            FixTransform();
        }

        public void FixTransform()
        {
            if (transform.lossyScale != Vector3.one)
            {
                Vector3 scale = Vector3.one;

                Transform parent = transform.parent;

                if (parent != null)
                {
                    scale.x /= parent.lossyScale.x;
                    scale.y /= parent.lossyScale.y;
                    scale.z /= parent.lossyScale.z;
                }

                transform.localScale = Vector3.one;
            }

            if (transform.position != Vector3.one)
            {
                transform.position = Vector3.zero;
            }

            if (transform.rotation != Quaternion.identity)
            {
                transform.rotation = Quaternion.identity;
            }
        }

        private void LateUpdate()
        {
            if (Lighting2D.disable)
            {
                return;
            }

            UpdateInternal();
        }


        public void UpdateInternal()
        {
            if (Lighting2D.disable)
            {
                return;
            }


            //TODO: Rendering.Manager.Main.InternalUpdate();
        }

        private void OnDisable()
        {
            sceneView.OnDisable();
        }


        private void OnEnable()
        {
            sceneView.OnEnable();

            Update();
            LateUpdate();
        }

      
    }
}