// 这是一个URP卡通渲染shader
// 主要实现效果：
// 1. 卡通风格的明暗过渡
// 2. 物体轮廓线
// 3. 颜色分层
Shader "Custom/URPCartoon"
{
    // Properties是在材质面板中显示的属性
    Properties
    {
        // [MainTexture]和[MainColor]是URP的标准命名
        [MainTexture] _BaseMap("主贴图", 2D) = "white" {}
        [MainColor] _BaseColor("主颜色", Color) = (1, 1, 1, 1)
        
        // 卡通渲染相关参数
        _RampThreshold ("明暗过渡阈值(越大越亮)", Range(0.1,1)) = 0.5
        _RampSmooth ("明暗过渡平滑度", Range(0,1)) = 0.1
        _OutlineColor ("轮廓线颜色", Color) = (0,0,0,1)
        _OutlineWidth ("轮廓线宽度", Range(0,0.1)) = 0.01
        _ColorSteps ("颜色分层数量", Range(2,8)) = 4
        
        // 这些是URP必需的属性，一般不需要修改
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
    }

    SubShader
    {
        // 指定这是URP shader，并设置渲染队列
        Tags { 
            "RenderType" = "Opaque"          // 不透明物体
            "RenderPipeline" = "UniversalPipeline"  // URP管线
            "Queue" = "Geometry"             // 正常不透明物体的渲染队列
        }
        
        // Pass 1: 轮廓线绘制
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" } // 不需要光照的Pass
            Cull Front // 剔除正面，只渲染背面
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // 顶点着色器输入结构
            struct Attributes
            {
                float4 positionOS : POSITION;    // 物体空间顶点位置
                float3 normalOS : NORMAL;        // 物体空间法线
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 顶点着色器输出结构（片段着色器输入）
            struct Varyings
            {
                float4 positionCS : SV_POSITION; // 裁剪空间位置
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _OutlineWidth;
            half4 _OutlineColor;
            
            // 顶点着色器：实现轮廓线
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // 沿法线方向扩展顶点位置，实现轮廓线
                float3 posOS = input.positionOS.xyz + input.normalOS * _OutlineWidth;
                output.positionCS = TransformObjectToHClip(posOS);
                return output;
            }

            // 片段着色器：设置轮廓线颜色
            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
        
        // Pass 2: 主要渲染Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" } // URP前向渲染
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // 启用阴影相关的关键字
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            // 顶点着色器输入
            struct Attributes
            {
                float4 positionOS : POSITION;    // 物体空间位置
                float3 normalOS : NORMAL;        // 物体空间法线
                float2 uv : TEXCOORD0;          // UV坐标
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 顶点着色器输出
            struct Varyings
            {
                float4 positionCS : SV_POSITION; // 裁剪空间位置
                float2 uv : TEXCOORD0;          // UV坐标
                float3 normalWS : TEXCOORD1;    // 世界空间法线
                float3 positionWS : TEXCOORD2;  // 世界空间位置
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // 声明纹理和采样器
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            // 声明材质属性
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _RampThreshold;
                float _RampSmooth;
                float _ColorSteps;
            CBUFFER_END
            
            // 顶点着色器
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // 转换顶点和法线到相应空间
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                // 计算UV坐标
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }
            
            // 片段着色器
            half4 frag(Varyings input) : SV_Target
            {
                // 采样主贴图
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 color = baseMap * _BaseColor;
                
                // 获取主光源信息
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                
                // 计算漫反射（半兰伯特光照模型）
                float NdotL = dot(normalWS, mainLight.direction) * 0.5 + 0.5;
                
                // 创建卡通渐变效果
                // smoothstep用于创建平滑的阶梯效果
                float ramp = smoothstep(_RampThreshold - _RampSmooth * 0.5, 
                                     _RampThreshold + _RampSmooth * 0.5, 
                                     NdotL);
                
                // 应用颜色分层
                // floor用于创建离散的颜色台阶
                ramp = floor(ramp * _ColorSteps) / _ColorSteps;
                
                // 计算最终颜色
                color.rgb *= mainLight.color * ramp;
                
                return color;
            }
            ENDHLSL
        }
        
        // Pass 3: 阴影投射Pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" } // 用于生成阴影贴图
            
            ZWrite On      // 开启深度写入
            ZTest LEqual   // 深度测试
            ColorMask 0    // 不输出颜色
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            float3 _LightDirection;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // 阴影投射的顶点着色器
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                // 应用阴影偏移，避免自阴影问题
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return output;
            }
            
            // 阴影投射的片段着色器
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    // 如果shader编译失败，使用URP的Lit shader作为后备
    FallBack "Universal Render Pipeline/Lit"
}
