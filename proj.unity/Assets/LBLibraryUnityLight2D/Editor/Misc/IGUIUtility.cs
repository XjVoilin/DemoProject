using UnityEngine;

namespace LIBII.Light2D
{
    internal interface IGUIUtility
    {
        int GetPermanentControlID();
        int hotControl { get; set; }
        int keyboardControl { get; set; }
        int GetControlID(int hint, FocusType focus);
    }

    internal class GUIUtilitySystem : IGUIUtility
    {
        public int GetPermanentControlID()
        {
            return -1;
        }

        public int hotControl
        {
            get { return GUIUtility.hotControl; }
            set { GUIUtility.hotControl = value; }
        }

        public int keyboardControl
        {
            get { return GUIUtility.keyboardControl; }
            set { GUIUtility.keyboardControl = value; }
        }

        public int GetControlID(int hint, FocusType focus)
        {
            return GUIUtility.GetControlID(hint, focus);
        }
    }
}