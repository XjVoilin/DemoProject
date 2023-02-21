using UnityEngine;

namespace LIBII.Light2D
{
    public class LightingMonoBehaviour : MonoBehaviour
    {
        public void DestroySelf()
        {
            if (Application.isPlaying)
            {
                Destroy(this.gameObject);
            }
            else
            {
                if (this != null && this.gameObject != null)
                {
                    DestroyImmediate(this.gameObject);
                }
            }
        }
    }
}