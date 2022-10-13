using UnityEngine;

namespace LIBII.Light2D
{
    public class DestroyTimer : MonoBehaviour
    {
        TimerHelper timer;

        void Start()
        {
            timer = TimerHelper.Create();
        }

        void Update()
        {
            if (timer.GetMillisecs() > 2000)
            {
                Destroy(gameObject);
            }
        }
    }
}