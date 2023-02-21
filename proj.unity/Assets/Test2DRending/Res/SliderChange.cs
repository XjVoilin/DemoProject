using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderChange : MonoBehaviour
{
   public Material mat;

   public void ChangeValue(float value)
   {
      mat.SetFloat("_StencilRef",value*255f);
   }

   public void ChangeCom(int com)
   {
      mat.SetFloat("_StencilComp",com);
   }
}
