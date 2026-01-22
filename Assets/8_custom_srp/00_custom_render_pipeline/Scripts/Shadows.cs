using UnityEngine;
using UnityEngine.Rendering;

namespace custom.render.pipeline {
    public class Shadows {
        const string bufferName = "Shadows";
        const int maxShadowedDirectionalLightCount = 4, maxShadowedOtherLightCount = 16;
        const int maxCascade = 4;

        static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
        static int dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
        static int otherShadowAtlasId = Shader.PropertyToID("_OtherShadowAtlas");
        static int otherShadowMatricesId = Shader.PropertyToID("_OtherShadowMatrices");
        static int otherShadowTilesId = Shader.PropertyToID("_OtherShadowTiles");


        static int cascadeCountId = Shader.PropertyToID("_CascadeCount");
        static int cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
        static int cascadeDataId = Shader.PropertyToID("_CascadeData");
        static int shadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");
        static int shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");
        static int shadowPancakingId = Shader.PropertyToID("_ShadowPancaking");


        static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascade],
            otherShadowMatrices = new Matrix4x4[maxShadowedOtherLightCount];

        static Vector4[] cascadeCullingSpheres = new Vector4[maxCascade],
            cascadeData = new Vector4[maxCascade],
            otherShadowTiles = new Vector4[maxShadowedOtherLightCount];

        static string[] directionalFilterKeywords = {
            "_DIRECTIONAL_PCF3",
            "_DIRECTIONAL_PCF5",
            "_DIRECTIONAL_PCF7",
        };

        static string[] cascadeBlendKeywords = {
            "_CASCADE_BLEND_SOFT",
            "_CASCADE_BLEND_DITHER",
        };

        static string[] shadowMaskKeywords = {
            "_SHADOW_MASK_ALWAYS",
            "_SHADOW_MASK_DISTANCE"
        };

        static string[] otherFilterKeywords = {
            "_OTHER_PCF3",
            "_OTHER_PCF5",
            "_OTHER_PCF7",
        };

        struct ShadowedDirectionalLight {
            public int visibleLightIndex;
            public float slopeScaleBias;
            public float nearPlaneOffset;
        }


        ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

        struct ShadowedOtherLight {
            public int visibleLightIndex;
            public float slopeScaleBias;
            public float normalBias;
            public bool isPoint;
        }

        ShadowedOtherLight[] shadowedOtherLights = new ShadowedOtherLight[maxShadowedOtherLightCount];

        CommandBuffer buffer = new CommandBuffer() {
            name = bufferName
        };

        ScriptableRenderContext context;
        CullingResults cullingResults;
        ShadowSettings settings;

        int shadowedDirLightCount, shadowedOtherLightCount;

        bool useShadowMask;

        Vector4 atlasSizes;

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings) {
            this.context = context;
            this.cullingResults = cullingResults;
            this.settings = settings;
            shadowedDirLightCount = shadowedOtherLightCount = 0;
            useShadowMask = false;
        }


        public void Cleanup() {
            buffer.ReleaseTemporaryRT(dirShadowAtlasId);
            if (shadowedOtherLightCount > 0) {
                buffer.ReleaseTemporaryRT(otherShadowAtlasId);
            }
            ExecuteBuffer();
        }


        /// 直射光源阴影设置
        /// 返回：光源阴影设置数据
        public Vector4 ReserveDirectionalShadows(Light light, int visibleLightIndex) {
            // 直射光源数量未超，有阴影，需要计算阴影
            if (shadowedDirLightCount < maxShadowedDirectionalLightCount &&
                light.shadows != LightShadows.None && light.shadowStrength > 0f) {
                // 阴影遮罩通道
                float maskChannel = -1;
                LightBakingOutput lightBaking = light.bakingOutput;

                // 混合光源 && 开启ShadowMask
                if (
                    lightBaking.lightmapBakeType == LightmapBakeType.Mixed &&
                    lightBaking.mixedLightingMode == MixedLightingMode.Shadowmask
                ) {
                    // 开启阴影遮罩
                    useShadowMask = true;
                    // 记录阴影遮罩通道
                    maskChannel = lightBaking.occlusionMaskChannel;
                }

                // 剔除范围内，无阴影投射器，使用烘焙阴影或者无阴影
                if (!cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)) {
                    return new Vector4(-light.shadowStrength, 0f, 0f, maskChannel);
                }

                // 当前直射光源设置数据
                shadowedDirectionalLights[shadowedDirLightCount] =
                    new ShadowedDirectionalLight {
                        visibleLightIndex = visibleLightIndex, // 光源索引
                        slopeScaleBias = light.shadowBias, // 光源对应斜率偏移，降低阴影痤疮
                        nearPlaneOffset = light.shadowNearPlane // 近平面偏差，调整大型物体投影
                    };
                // 光源阴影设置数据
                return new Vector4(
                    light.shadowStrength, // 光源阴影强度
                    settings.directional.cascadeCount * shadowedDirLightCount++, // 光源对应光照贴图 tile 索引
                    light.shadowNormalBias,// 光源法线偏移
                    maskChannel // 阴影遮罩通道
                );
            }


            return new Vector4(0f, 0f, 0f, -1f);
        }

        // 点光源或探照灯阴影设置
        // 返回：点光源阴影设置数据
        public Vector4 ReserveOtherShadows(Light light, int visibleLightIndex) {

            if (light.shadows == LightShadows.None || light.shadowStrength <= 0f) {
                return new Vector4(0f, 0f, 0f, -1f);
            }

            float maskChannel = -1f;

            LightBakingOutput lightBaking = light.bakingOutput;
            if (
                lightBaking.lightmapBakeType == LightmapBakeType.Mixed &&
                lightBaking.mixedLightingMode == MixedLightingMode.Shadowmask
            ) {
                useShadowMask = true;
                maskChannel = lightBaking.occlusionMaskChannel;
            }

            bool isPoint = light.type == LightType.Point;
            int newLightCount = shadowedOtherLightCount + (isPoint ? 6 : 1);

            if (newLightCount > maxShadowedOtherLightCount ||
                !cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)) {
                return new Vector4(-light.shadowStrength, 0f, 0f, maskChannel);
            }

            shadowedOtherLights[shadowedOtherLightCount] = new ShadowedOtherLight {
                visibleLightIndex = visibleLightIndex,
                slopeScaleBias = light.shadowBias,
                normalBias = light.shadowNormalBias,
                isPoint = isPoint
            };

            Vector4 data = new Vector4(
                light.shadowStrength, shadowedOtherLightCount,
                isPoint ? 1f : 0f, maskChannel
            );
            shadowedOtherLightCount = newLightCount;
            return data;
        }





        public void Render() {
            if (shadowedDirLightCount > 0) {
                RenderDirectionalShadows();
            } else {
                buffer.GetTemporaryRT(
                    dirShadowAtlasId, 1, 1,
                    32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
                );
            }

            if (shadowedOtherLightCount > 0) {
                RenderOtherShadows();
            } else {
                buffer.SetGlobalTexture(otherShadowAtlasId, dirShadowAtlasId);
            }

            buffer.BeginSample(bufferName);
            SetKeywords(shadowMaskKeywords, useShadowMask ?
                QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask ? 0 : 1 :
                -1);

            // 存在全局光才开启级联
            buffer.SetGlobalInt(cascadeCountId, shadowedDirLightCount > 0 ? settings.directional.cascadeCount : 0);

            float f = 1f - settings.directional.cascadeFade;
            buffer.SetGlobalVector(
                shadowDistanceFadeId, new Vector4(
                    1f / settings.maxDistance, 1f / settings.distanceFade,
                    1f / (1f - f * f)
                )
            );
            buffer.SetGlobalVector(shadowAtlasSizeId, atlasSizes);
            buffer.EndSample(bufferName);
            ExecuteBuffer();
        }

        // 直射光源嘤嘤渲染
        void RenderDirectionalShadows() {

            // 阴影贴图尺寸
            int atlasSize = (int)settings.directional.atlasSize;
            atlasSizes.x = atlasSize;
            atlasSizes.y = 1f / atlasSize;

            // 创建阴影贴图
            buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

            // 设置渲染目标
            buffer.SetRenderTarget(
                dirShadowAtlasId, // 阴影贴图 缓冲区
                RenderBufferLoadAction.DontCare, // 要清屏，无需关心
                RenderBufferStoreAction.Store
            );
            // 清屏
            buffer.ClearRenderTarget(true, false, Color.clear);
            // 启用影子煎饼吸附
            buffer.SetGlobalFloat(shadowPancakingId, 1f);
            buffer.BeginSample(bufferName);
            ExecuteBuffer();
            // 图集 tile 数量
            int tiles = shadowedDirLightCount * settings.directional.cascadeCount;
            int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
            int tileSize = atlasSize / split;

            // 循环渲染每个光源图集
            for (int i = 0; i < shadowedDirLightCount; i++) {
                RenderDirectionalShadows(i, split, tileSize);
            }

            // 传递级联数据
            buffer.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
            buffer.SetGlobalVectorArray(cascadeDataId, cascadeData);
            buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);

            SetKeywords(directionalFilterKeywords, (int)settings.directional.filter - 1);
            SetKeywords(cascadeBlendKeywords, (int)settings.directional.cascadeBlend - 1);

            buffer.EndSample(bufferName);
            ExecuteBuffer();
        }



        void RenderDirectionalShadows(int index, int split, int tileSize) {
            // 光源数据
            ShadowedDirectionalLight light = shadowedDirectionalLights[index];
            // 索引光源阴影绘制设置
            ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(
                    cullingResults, light.visibleLightIndex,  // 剔除结构集，光源索引
                    BatchCullingProjectionType.Orthographic // 正交方式
            );

            int cascadeCount = settings.directional.cascadeCount;
            int tileOffset = index * cascadeCount;
            Vector3 ratios = settings.directional.CascadeRatios;

            // 控制剔除范围，处理过渡
            float cullingFactor = Mathf.Max(0f, 0.8f - settings.directional.cascadeFade);

            float tileScale = 1f / split;

            // 渲染光源级联阴影贴图
            for (int i = 0; i < cascadeCount; i++) {

                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                    // 可见光索引
                    light.visibleLightIndex,
                    // 阴影级联参数
                    i, cascadeCount, ratios,
                    // 纹理尺寸
                    tileSize,
                    // 近平面
                    light.nearPlaneOffset,
                    // 输出参数：视图矩阵、投影矩阵
                    out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                    // 阴影结构体 剔除投影投影阴影对象信息
                    out ShadowSplitData splitData
                );


                splitData.shadowCascadeBlendCullingFactor = cullingFactor;
                shadowSettings.splitData = splitData;

                if (index == 0) {
                    SetCascadeData(i, splitData.cullingSphere, tileSize);
                }

                int tileIndex = tileOffset + i;
                dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(
                    projectionMatrix * viewMatrix,
                    SetTileViewport(tileIndex, split, tileSize),
                    tileScale
                );
                buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                buffer.SetGlobalDepthBias(0f, light.slopeScaleBias);
                ExecuteBuffer();
                context.DrawShadows(ref shadowSettings);
                buffer.SetGlobalDepthBias(0f, 0f);
            }
        }





        void SetCascadeData(int index, Vector4 cullingSphere, float tileSize) {
            float texelSize = 2f * cullingSphere.w / tileSize;
            float filterSize = texelSize * ((float)settings.directional.filter + 1f);
            // 剔除空间缩小采样核范围，采样核是方形的，所以直接缩小方形区域即可
            cullingSphere.w -= filterSize;
            cullingSphere.w *= cullingSphere.w;

            cascadeCullingSpheres[index] = cullingSphere;
            cascadeData[index] = new Vector4(
                1f / cullingSphere.w,
                filterSize * 1.4142136f
            );
        }



        void RenderOtherShadows() {
            int atlasSize = (int)settings.other.atlasSize;
            atlasSizes.z = atlasSize;
            atlasSizes.w = 1f / atlasSize;
            buffer.GetTemporaryRT(
                otherShadowAtlasId, atlasSize, atlasSize,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
            buffer.SetRenderTarget(
                otherShadowAtlasId,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
            );
            buffer.ClearRenderTarget(true, false, Color.clear);
            buffer.BeginSample(bufferName);
            ExecuteBuffer();

            int tiles = shadowedOtherLightCount;
            int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
            int tileSize = atlasSize / split;

            for (int i = 0; i < shadowedOtherLightCount;) {
                if (shadowedOtherLights[i].isPoint) {
                    RenderPointShadows(i, split, tileSize);
                    i += 6;
                } else {
                    RenderSpotShadows(i, split, tileSize);
                    i += 1;
                }
            }


            buffer.SetGlobalMatrixArray(otherShadowMatricesId, otherShadowMatrices);
            buffer.SetGlobalVectorArray(otherShadowTilesId, otherShadowTiles);
            SetKeywords(
                otherFilterKeywords, (int)settings.other.filter - 1
            );
            // 禁用影子煎饼吸附
            buffer.SetGlobalFloat(shadowPancakingId, 0f);

            buffer.EndSample(bufferName);
            ExecuteBuffer();
        }

        void RenderPointShadows(int index, int split, int tileSize) {
            ShadowedOtherLight light = shadowedOtherLights[index];
            var shadowSettings = new ShadowDrawingSettings(
                cullingResults, light.visibleLightIndex,
                BatchCullingProjectionType.Perspective
            );

            // 立方体贴图 FOV = 90 ，m00 = 1 省略
            float texelSize = 2f / tileSize;
            float filterSize = texelSize * ((float)settings.other.filter + 1f);
            float bias = light.normalBias * filterSize * 1.4142136f;
            float tileScale = 1f / split;

            float fovBias = 
                Mathf.Atan(1f +bias + filterSize) * Mathf.Rad2Deg * 2f - 90;

            for (int i = 0; i < 6; i++) {
                cullingResults.ComputePointShadowMatricesAndCullingPrimitives(
                    light.visibleLightIndex, (CubemapFace)i, fovBias,
                    out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                    out ShadowSplitData splitData
                );

                viewMatrix.m11 = -viewMatrix.m11;
                viewMatrix.m12 = -viewMatrix.m12;
                viewMatrix.m13 = -viewMatrix.m13;

                shadowSettings.splitData = splitData;
                int tileIndex = index + i;
                Vector2 offset = SetTileViewport(tileIndex, split, tileSize);
                SetOtherTileData(tileIndex, offset, tileScale, bias);
                otherShadowMatrices[tileIndex] = ConvertToAtlasMatrix(
                    projectionMatrix * viewMatrix, offset, tileScale
                );

                buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                buffer.SetGlobalDepthBias(0f, light.slopeScaleBias);
                ExecuteBuffer();
                context.DrawShadows(ref shadowSettings);
                buffer.SetGlobalDepthBias(0f, 0f);
            }
        }


        void RenderSpotShadows(int index, int split, int tileSize) {
            ShadowedOtherLight light = shadowedOtherLights[index];
            var shadowSettings = new ShadowDrawingSettings(
                cullingResults, light.visibleLightIndex,
                BatchCullingProjectionType.Perspective
            );
            cullingResults.ComputeSpotShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, out Matrix4x4 viewMatrix,
                out Matrix4x4 projectionMatrix, out ShadowSplitData splitData
            );
            shadowSettings.splitData = splitData;
            // 聚光灯为正方形aspect，只需取宽度
            // ndc 空间 范围 [-1,1] 范围是2
            // 光源阴影贴图尺寸 tileSize
            // 因为，projectionMatrix.m00 = 2n / width => width = 2n / projectionMatrix.m00, 单位距离 n = 1
            // 所以，width = 2 / projectionMatrix.m00
            float texelSize = 2f / (tileSize * projectionMatrix.m00); // 计算单位深度的纹素大小
            float filterSize = texelSize * ((float)settings.other.filter + 1);
            float bias = light.normalBias * filterSize * 1.4142136f;

            Vector2 offset = SetTileViewport(index, split, tileSize);
            float tileScale = 1f / split;
            SetOtherTileData(index, offset, tileScale, bias);

            // 图集转换矩阵
            otherShadowMatrices[index] = ConvertToAtlasMatrix(
                projectionMatrix * viewMatrix, // 世界空间 -> 视图空间 -> 投影空间
                offset, tileScale
            );
            // 视图空间转换到裁剪空间矩阵
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            // 深度偏差，法线偏差
            buffer.SetGlobalDepthBias(0f, light.slopeScaleBias);
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
            buffer.SetGlobalDepthBias(0f, 0f);
        }

        void SetOtherTileData(int index, Vector2 offset, float scale, float bias) {
            // 半个纹素尺寸
            float border = atlasSizes.w * 0.5f;
            Vector4 data;
            // x/y tile 左下脚起点，往内缩半个纹素
            data.x = offset.x * scale + border;
            data.y = offset.y * scale + border;
            // tile的可用边长，避免 PCF 采样到邻接 tile 
            data.z = scale - border - border;
            data.w = bias;// 法线偏差
            // 其他光源阴影贴图数据
            otherShadowTiles[index] = data;
        }


        Vector2 SetTileViewport(int index, int split, float tileSize) {
            // tile 偏移量
            Vector2 offset = new Vector2(index % split, index / split);
            buffer.SetViewport(
                new Rect(
                    offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
                )
            );
            return offset;
        }

        Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, float scale) {
            // 反向
            if (SystemInfo.usesReversedZBuffer) {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);
            return m;
        }



        void SetKeywords(string[] keywords, int enabledIndex) {
            for (int i = 0; i < keywords.Length; i++) {
                if (i == enabledIndex) {
                    buffer.EnableShaderKeyword(keywords[i]);
                } else {
                    buffer.DisableShaderKeyword(keywords[i]);
                }
            }
        }


        void ExecuteBuffer() {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }





    }
}