using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour {

    public float f_UpdateInterval = 0.5F;

    private float f_LastInterval;

    private int i_Frames = 0;

    private float f_Fps;

    void Start() 
    {
        Application.targetFrameRate=60;

        f_LastInterval = Time.realtimeSinceStartup;
        i_Frames = 0;

        DontDestroyOnLoad (gameObject);

	
    }

    void OnGUI() 
    {
        GUI.skin.label.normal.textColor = new Color (1f, 0f, 0f, 1f);
        GUI.skin.label.fontSize=50;
        GUI.Label(new Rect(10, 10, 600, 200), "FPS:" + f_Fps.ToString("f2"));
    }

    void Update() 
    {
        ++i_Frames;

        if (Time.realtimeSinceStartup > f_LastInterval + f_UpdateInterval) 
        {
            f_Fps = i_Frames / (Time.realtimeSinceStartup - f_LastInterval);

            i_Frames = 0;

            f_LastInterval = Time.realtimeSinceStartup;
        }
    }
}
