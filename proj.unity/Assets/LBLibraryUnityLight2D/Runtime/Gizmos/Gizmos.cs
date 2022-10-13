namespace LIBII.Light2D
{
#if UNITY_EDITOR
    [System.Serializable]
    public class Gizmos
    {
        public EditorDrawGizmos drawGizmos = EditorDrawGizmos.Selected;
        public EditorGizmosBounds drawGizmosBounds = EditorGizmosBounds.Disabled;
        public EditorChunks drawGizmosChunks = EditorChunks.Disabled;

        public EditorIcons drawIcons = EditorIcons.Disabled;

        public int sceneViewLayer = 0;

        public int gameViewLayer = 0;
    }
#endif
}