# Custom SRP 渲染管线流程

## 1. CPU端渲染流程 (CameraRenderer.cs)

### 1.1 Render() 主入口
- **输入**: ScriptableRenderContext, Camera, 渲染配置参数
- **流程**:
  ```
  Render()
  ├── PrepareBuffer() - 准备命令缓冲区
  ├── PrepareForSceneWindow() - 场景窗口准备
  ├── Cull() - 视锥体剔除
  │   └── 设置阴影距离 = Min(maxShadowDistance, camera.farClipPlane)
  ├── Setup() - 相机设置
  │   ├── SetupCameraProperties() - 设置相机属性
  │   └── ClearRenderTarget() - 清除渲染目标（深度/颜色）
  ├── lighting.Setup() - 光照和阴影设置
  ├── DrawVisibleGeometry() - 绘制可见几何体
  │   ├── 配置DrawingSettings
  │   │   ├── ShaderPass: "SRPDefaultUnlit"
  │   │   ├── ShaderPass: "CustomLit"
  │   │   ├── enableDynamicBatching
  │   │   ├── enableInstancing
  │   │   └── perObjectData (反射探针、光照贴图、阴影遮罩等)
  │   ├── 绘制不透明物体 (RenderQueueRange.opaque)
  │   ├── DrawSkybox() - 绘制天空盒
  │   └── 绘制透明物体 (RenderQueueRange.transparent)
  ├── lighting.Cleanup() - 清理光照资源
  └── Submit() - 提交渲染命令到GPU
  ```

---

## 2. GPU端渲染流程 (Lit.shader)

### 2.1 Shader属性定义
```
Properties
├── 基础纹理
│   ├── _BaseMap - 基础纹理
│   └── _BaseColor - 基础颜色
├── 透明度控制
│   ├── _Cutoff - Alpha裁剪阈值
│   ├── _Clipping - Alpha裁剪开关
│   └── _Shadows - 阴影模式 (On/Clip/Dither/Off)
├── 复杂贴图
│   ├── _MaskMap - MODS贴图 (Metallic/Occlusion/Detail/Smoothness)
│   ├── _NormalMap - 法线贴图
│   ├── _EmissionMap - 自发光贴图
│   └── _DetailMap - 细节贴图
├── 材质属性
│   ├── _Metallic - 金属度
│   ├── _Occlusion - 环境光遮蔽
│   ├── _Smoothness - 光滑度
│   └── _Fresnel - 菲涅尔强度
└── 混合模式
    ├── _SrcBlend - 源混合模式
    ├── _DstBlend - 目标混合模式
    └── _ZWrite - 深度写入
```

---

### 2.2 Pass 1: CustomLit (主渲染通道)

#### 2.2.1 顶点着色器 - LitPassVertex
```
输入 (Attributes)
├── positionOS - 对象空间位置
├── normalOS - 对象空间法线
├── tangentOS - 对象空间切线
├── baseUV - 基础UV坐标
└── GI_ATTRIBUTE_DATA - 全局光照数据 (光照贴图UV等)

处理流程
├── UNITY_SETUP_INSTANCE_ID - 设置实例ID (GPU Instancing)
├── 空间变换
│   ├── positionWS = TransformObjectToWorld(positionOS) - 转世界空间
│   ├── positionCS = TransformWorldToHClip(positionWS) - 转裁剪空间
│   └── normalWS = TransformObjectToWorldNormal(normalOS) - 法线转世界空间
├── 切线空间处理 (#if _NORMAL_MAP)
│   └── tangentWS = TransformObjectToWorldDir(tangentOS)
├── UV变换
│   ├── baseUV = TransformBaseUV(baseUV) - 基础UV变换
│   └── detailUV = TransformDetailUV(baseUV) - 细节UV变换 (#if _DETAIL_MAP)
└── TRANSFER_GI_DATA - 传递全局光照数据

输出 (Varyings)
├── positionCS - 裁剪空间位置 (SV_POSITION)
├── positionWS - 世界空间位置
├── normalWS - 世界空间法线
├── tangentWS - 世界空间切线 (#if _NORMAL_MAP)
├── baseUV - 基础UV
├── detailUV - 细节UV (#if _DETAIL_MAP)
└── GI_VARYINGS_DATA - 全局光照插值数据
```

#### 2.2.2 片元着色器 - LitPassFragment
```
输入 (Varyings)
└── [从顶点着色器传递的插值数据]

处理流程
├── 1. LOD淡入淡出处理
│   └── ClipLOD(positionCS.xy, unity_LODFade.x)
│
├── 2. 配置输入
│   ├── GetInputConfig(baseUV)
│   ├── 启用遮罩贴图 (#if _MASK_MAP)
│   └── 启用细节贴图 (#if _DETAIL_MAP)
│
├── 3. 基础颜色和Alpha裁剪
│   ├── base = GetBase(config) - 获取基础颜色
│   └── clip(base.a - GetCutoff(config)) - Alpha裁剪 (#if _CLIPPING)
│
├── 4. 构建Surface数据结构
│   ├── position - 世界空间位置
│   ├── normal - 世界空间法线
│   │   ├── 法线贴图: NormalTangentToWorld() (#if _NORMAL_MAP)
│   │   └── 顶点法线: normalize(normalWS)
│   ├── viewDirection - 视线方向
│   ├── depth - 视空间深度
│   ├── color - 基础颜色
│   ├── alpha - 透明度
│   ├── metallic - 金属度
│   ├── occlusion - 环境光遮蔽
│   ├── smoothness - 光滑度
│   ├── fresnelStrength - 菲涅尔强度
│   └── dither - 抖动噪声
│
├── 5. 计算BRDF (双向反射分布函数)
│   ├── GetBRDF(surface) - 标准BRDF
│   └── GetBRDF(surface, true) - 预乘Alpha (#if _PREMULTIPLY_ALPHA)
│
├── 6. 全局光照 (GI)
│   └── gi = GetGI(GI_FRAGMENT_DATA, surface, brdf)
│       ├── 光照贴图采样
│       ├── 光照探针采样
│       └── 反射探针采样
│
├── 7. 光照计算
│   └── color = GetLighting(surface, brdf, gi)
│       ├── 直接光照 (方向光、点光源、聚光灯)
│       ├── 阴影计算
│       └── 间接光照 (GI)
│
└── 8. 自发光
    └── color += GetEmission(config)

输出
└── float4(color, alpha) - 最终颜色 (SV_TARGET)
```

#### 2.2.3 Shader变体和多编译指令
```
#pragma shader_feature
├── _CLIPPING - Alpha裁剪
├── _PREMULTIPLY_ALPHA - 预乘Alpha
├── _RECEIVE_SHADOWS - 接收阴影
├── _NORMAL_MAP - 法线贴图
├── _MASK_MAP - 遮罩贴图
└── _DETAIL_MAP - 细节贴图

#pragma multi_compile
├── _DIRECTIONAL_PCF3/_DIRECTIONAL_PCF5/_DIRECTIONAL_PCF7 - 方向光PCF滤波
├── _CASCADE_BLEND_SOFT/_CASCADE_BLEND_DITHER - 级联阴影混合
├── _SHADOW_MASK_ALWAYS/_SHADOW_MASK_DISTANCE - 阴影遮罩模式
├── LOD_FADE_CROSSFADE - LOD交叉淡入淡出
├── LIGHTMAP_ON - 光照贴图
├── _LIGHTS_PER_OBJECT - 逐对象光照
└── _OTHER_PCF3/_OTHER_PCF5/_OTHER_PCF7 - 其他光源PCF滤波
```

---

### 2.3 Pass 2: ShadowCaster (阴影投射通道)

#### 2.3.1 顶点着色器 - ShadowCasterPassVertex
```
输入 (Attributes)
├── positionOS - 对象空间位置
└── baseUV - 基础UV坐标

处理流程
├── UNITY_SETUP_INSTANCE_ID - 设置实例ID
├── 空间变换
│   ├── positionWS = TransformObjectToWorld(positionOS)
│   └── positionCS = TransformWorldToHClip(positionWS)
├── Shadow Pancaking (阴影扁平化优化)
│   └── 将顶点钳制到近裁剪面 (#if _ShadowPancaking)
│       ├── UNITY_REVERSED_Z: positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE)
│       └── 否则: positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE)
└── UV变换
    └── baseUV = TransformBaseUV(baseUV)

输出 (Varyings)
├── positionCS - 裁剪空间位置
└── baseUV - 基础UV
```

#### 2.3.2 片元着色器 - ShadowCasterPassFragment
```
输入 (Varyings)
└── [从顶点着色器传递的数据]

处理流程
├── UNITY_SETUP_INSTANCE_ID - 设置实例ID
├── LOD淡入淡出
│   └── ClipLOD(positionCS.xy, unity_LODFade.x)
├── 获取基础颜色
│   └── base = GetBase(GetInputConfig(baseUV))
└── Alpha处理
    ├── _SHADOWS_CLIP: clip(base.a - GetCutoff(config)) - 硬裁剪
    └── _SHADOWS_DITHER: clip(base.a - dither) - 抖动裁剪

输出
└── void (仅写入深度，ColorMask 0)
```

#### 2.3.3 Shader变体
```
#pragma shader_feature
└── _SHADOWS_CLIP/_SHADOWS_DITHER - 阴影裁剪模式

#pragma multi_compile
└── LOD_FADE_CROSSFADE - LOD交叉淡入淡出
```

---

### 2.4 Pass 3: Meta (光照贴图烘焙通道)

#### 2.4.1 顶点着色器 - MetaPassVertex
```
输入 (Attributes)
├── positionOS - 对象空间位置
├── baseUV - 基础UV坐标
└── lightMapUV - 光照贴图UV坐标

处理流程
├── UV空间变换
│   └── positionOS.xy = lightMapUV * unity_LightmapST.xy + unity_LightmapST.zw
│       (将光照贴图UV映射到位置，用于烘焙)
├── Z坐标处理
│   └── positionOS.z = (positionOS.z > 0.0) ? FLT_MIN : 0.0
├── 空间变换
│   └── positionCS = TransformWorldToHClip(positionOS)
└── UV变换
    └── baseUV = TransformBaseUV(baseUV)

输出 (Varyings)
├── positionCS - 裁剪空间位置
└── baseUV - 基础UV
```

#### 2.4.2 片元着色器 - MetaPassFragment
```
输入 (Varyings)
└── [从顶点着色器传递的数据]

处理流程
├── 1. 获取基础数据
│   ├── config = GetInputConfig(baseUV)
│   └── base = GetBase(config)
│
├── 2. 构建Surface数据
│   ├── color - 基础颜色
│   ├── metallic - 金属度
│   └── smoothness - 光滑度
│
├── 3. 计算BRDF
│   └── brdf = GetBRDF(surface)
│
└── 4. 根据烘焙请求输出数据
    ├── unity_MetaFragmentControl.x (漫反射率请求)
    │   └── meta = float4(brdf.diffuse + brdf.specular * brdf.roughness * 0.5, 1.0)
    │       └── 应用输出增强: PositivePow(meta.rgb, unity_OneOverOutputBoost)
    └── unity_MetaFragmentControl.y (自发光请求)
        └── meta = float4(GetEmission(config), 1.0)

输出
└── float4 meta - 烘焙数据 (SV_TARGET)
```

---

## 3. 渲染管线数据流

```
CPU端 (CameraRenderer.cs)
│
├─→ 1. 视锥体剔除 (Cull)
│   └─→ CullingResults (可见对象列表)
│
├─→ 2. 光照设置 (Lighting.Setup)
│   ├─→ 设置光源数据 (方向光、点光源、聚光灯)
│   ├─→ 设置阴影数据 (阴影贴图、级联参数)
│   └─→ 上传到GPU常量缓冲区
│
├─→ 3. 绘制阴影贴图
│   └─→ 调用ShadowCaster Pass
│       ├─→ ShadowCasterPassVertex (顶点变换)
│       └─→ ShadowCasterPassFragment (Alpha处理 + 深度写入)
│
├─→ 4. 绘制不透明物体
│   └─→ 调用CustomLit Pass
│       ├─→ LitPassVertex (顶点变换 + 数据传递)
│       └─→ LitPassFragment (完整光照计算)
│           ├─→ Surface数据构建
│           ├─→ BRDF计算
│           ├─→ GI采样 (光照贴图/光照探针/反射探针)
│           ├─→ 直接光照计算 (含阴影)
│           └─→ 最终颜色输出
│
├─→ 5. 绘制天空盒
│
├─→ 6. 绘制透明物体
│   └─→ 调用CustomLit Pass (同不透明物体)
│
└─→ 7. 光照贴图烘焙 (离线/编辑器)
    └─→ 调用Meta Pass
        ├─→ MetaPassVertex (UV空间变换)
        └─→ MetaPassFragment (输出漫反射率或自发光)
```

---

## 4. 关键技术点

### 4.1 GPU Instancing
- 通过 `UNITY_VERTEX_INPUT_INSTANCE_ID` 和 `UNITY_SETUP_INSTANCE_ID` 实现
- 允许单次Draw Call绘制多个相同网格的实例

### 4.2 动态批处理
- 通过 `enableDynamicBatching` 启用
- 合并小型网格以减少Draw Call

### 4.3 阴影技术
- **级联阴影贴图**: 多个不同分辨率的阴影贴图覆盖不同距离
- **PCF滤波**: 3x3、5x5、7x7采样实现软阴影
- **Shadow Pancaking**: 优化阴影投射性能
- **阴影遮罩**: 结合烘焙和实时阴影

### 4.4 全局光照 (GI)
- **光照贴图**: 烘焙的静态光照
- **光照探针**: 动态对象的间接光照
- **反射探针**: 环境反射

### 4.5 LOD系统
- **LOD_FADE_CROSSFADE**: LOD级别之间的平滑过渡
- **ClipLOD**: 使用抖动实现淡入淡出效果

### 4.6 材质特性
- **法线贴图**: 增加表面细节
- **遮罩贴图 (MODS)**:
  - M: Metallic (金属度)
  - O: Occlusion (环境光遮蔽)
  - D: Detail Mask (细节遮罩)
  - S: Smoothness (光滑度)
- **细节贴图**: 近距离额外细节
- **自发光**: 物体自身发光

### 4.7 透明度处理
- **Alpha Clipping**: 硬边缘透明 (clip函数)
- **Alpha Blending**: 混合透明 (Blend模式)
- **Premultiplied Alpha**: 预乘Alpha混合
- **Dithering**: 抖动透明 (用于阴影)

---

## 5. Shader编译变体策略

### 5.1 Shader Feature (按材质启用)
- 仅编译材质实际使用的变体
- 用于材质特定功能 (法线贴图、遮罩贴图等)

### 5.2 Multi Compile (全局编译)
- 编译所有可能的组合
- 用于场景级功能 (光照贴图、阴影质量等)

### 5.3 变体数量控制
```
CustomLit Pass变体数 =
  shader_feature变体 (2^6 = 64) ×
  multi_compile变体 (3 × 3 × 3 × 2 × 2 × 2 × 4) =
  约 27,648 个变体 (实际会更少，因为有些组合不兼容)
```

---

## 6. 性能优化要点

### 6.1 CPU端
- **视锥体剔除**: 减少提交到GPU的对象数量
- **批处理**: 减少Draw Call数量
- **GPU Instancing**: 单次Draw Call绘制多个实例

### 6.2 GPU端
- **Early-Z**: 深度预通道优化
- **Alpha Test优化**: 使用Dithering代替硬裁剪
- **LOD系统**: 远距离使用低精度模型
- **Shader变体剪枝**: 只编译需要的变体

### 6.3 内存优化
- **纹理压缩**: 减少显存占用
- **Mipmap**: 减少纹理采样带宽
- **阴影贴图分辨率**: 平衡质量和性能
