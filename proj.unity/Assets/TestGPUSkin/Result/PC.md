#模型动画性能测试:

## 前提

-[x] **关闭阴影**
-[x] **所有测试保留Animator组件**

##参数:
**单个模型:2958个顶点,1486个面,80个骨骼**  
**整个场景:1280个模型,380W个顶点,190W个面**

## 测试

- ###**<font color=red>不开启</font>Unity GPU Skinning,使用Skinnd Mesh Render组件,开启GPUInstancing**
 
>**帧率: 8.7**
>
>**Draw Calls: 1281** 

![](Img/1.bmp)
---

- ###**<font color=red>开启</font>Unity GPU Skinning,使用Skinnd Mesh Render组件,开启GPUInstancing**

>**帧率: 14.2**  
>**Draw Calls: 1281** 

![](Img/2.bmp)

- ###**<font color=red>替换SkinnedMeshRender</font>组件为MeshRender+MeshFilter,不开启GPUInstancing,<font color=yellow>无动画表现</font>**

>**帧率: 40.2**  
>**Draw Calls: 1281** 

![](Img/3.bmp)

- ###替换SkinnedMeshRender组件为MeshRender+MeshFilter,<font color=red>开启</font>GPUInstancing,<font color=yellow>无动画表现</font>

>**帧率: 35.3**  
>**Draw Calls: 4** 

![](Img/4.bmp)

- ###使用自定义GPU Skinning,<font color = red>MeshRender+SelfGPUSkin</font>

>**帧率: 28.3**\
>**Draw call: 4**