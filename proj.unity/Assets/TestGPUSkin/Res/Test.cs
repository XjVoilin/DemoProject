using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        
        Debug.Log(skinnedMeshRenderer.bones.Length);
    }
}
