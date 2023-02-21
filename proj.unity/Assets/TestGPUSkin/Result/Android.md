
##参数:
**单个模型:2958个顶点,1486个面,80个骨骼**  
**整个场景:340个模型,100W个顶点,50.5W个面**

## 测试 安卓 (三星 高端机)

- ###**<font color=red>不开启</font>Unity GPU Skinning,使用Skinnd Mesh Render组件,开启GPUInstancing**
 > **帧率: 16**\
 > **DrawCall: 342**
 > **包体大小: 37.9M**

- ###**<font color=red>开启</font>Unity GPU Skinning,使用Skinnd Mesh Render组件,开启GPUInstancing**
> **帧率: 31.9**\
> **DrawCall: 342**
> **包体大小: 37.9M**

- ###**<font color=red>替换SkinnedMeshRender</font>组件为MeshRender+MeshFilter,不开启GPUInstancing,<font color=yellow>无动画表现</font>**
> **帧率: 60**\
> **DrawCall: 342**
> **包体大小: 34.9M**

- ###**<font color=red>替换SkinnedMeshRender</font>组件为MeshRender+MeshFilter,开启GPUInstancing,<font color=yellow>无动画表现</font>**
> **帧率: 60**\
> **DrawCall: 5**
> **包体大小: 36.4M**

- ###使用自定义GPU Skinning,<font color = red>MeshRender+SelfGPUSkin</font>
> **帧率: 60**\
> **DrawCall: 5**
> **包体大小: 34.9M**

## 测试 安卓 (OPPO Reno2Z)

- ###**<font color=red>不开启</font>Unity GPU Skinning,使用Skinnd Mesh Render组件,开启GPUInstancing**
> **帧率: 28**\
> **DrawCall: 342**
> **包体大小: 37.9M**

- ###**<font color=red>开启</font>Unity GPU Skinning,使用Skinnd Mesh Render组件,开启GPUInstancing**
> **帧率: 15**\
> **DrawCall: 342**
> **包体大小: 37.9M**

- ###**<font color=red>替换SkinnedMeshRender</font>组件为MeshRender+MeshFilter,不开启GPUInstancing,<font color=yellow>无动画表现</font>**
> **帧率: 60**\
> **DrawCall: 342**
> **包体大小: 34.9M**

- ###**<font color=red>替换SkinnedMeshRender</font>组件为MeshRender+MeshFilter,开启GPUInstancing,<font color=yellow>无动画表现</font>**
> **帧率: 29**\
> **DrawCall: 3**
> **包体大小: 36.4M**

- ###使用自定义GPU Skinning,<font color = red>MeshRender+SelfGPUSkin</font>
> **帧率: 35**\
> **DrawCall: 3**
> **包体大小: 34.9M**

## 测试 安卓 (OPPO A5)

- ###**<font color=red>不开启</font>Unity GPU Skinning,使用Skinnd Mesh Render组件,开启GPUInstancing**
> **帧率: 21**\
> **DrawCall: 342**
> **包体大小: 37.9M**

- ###**<font color=red>开启</font>Unity GPU Skinning,使用Skinnd Mesh Render组件,开启GPUInstancing**
> **帧率: 5.9**\
> **DrawCall: 342**
> **包体大小: 37.9M**

- ###**<font color=red>替换SkinnedMeshRender</font>组件为MeshRender+MeshFilter,不开启GPUInstancing,<font color=yellow>无动画表现</font>**
> **帧率: 15**\
> **DrawCall: 342**
> **包体大小: 34.9M**

- ###**<font color=red>替换SkinnedMeshRender</font>组件为MeshRender+MeshFilter,开启GPUInstancing,<font color=yellow>无动画表现</font>**
> **帧率: 25**\
> **DrawCall: 5**
> **包体大小: 36.4M**

- ###使用自定义GPU Skinning,<font color = red>MeshRender+SelfGPUSkin</font>
> **帧率: 29**\
> **DrawCall: 5**
> **包体大小: 34.9M**