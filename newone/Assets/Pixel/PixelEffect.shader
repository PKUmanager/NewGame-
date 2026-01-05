Shader "Custom/PixelateEffect_Quad"
{
    Properties
    {
        // 这里的属性被清空了，强制 Shader 只听脚本的命令
        // 这样可以完美解决“脚本修改数值无效”的问题
        [HideInInspector] _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        // Queue=Overlay：确保画在所有物体（包括 Sprite 和 UI）的最上层
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        Blend One Zero
        ZTest Always 
        ZWrite Off 
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            // 全局变量，由 C# 脚本控制
            float _PixelSize;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vertexInput.positionCS;
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // 1. 获取屏幕坐标
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                // 2. 计算像素化网格
                float pSize = max(1.0, _PixelSize);
                float2 gridRes = _ScreenParams.xy / pSize;
                float2 pixelUV = floor(screenUV * gridRes) / gridRes;

                // 3. 抓取背后的屏幕画面 (RGB)
                float3 sceneColor = SampleSceneColor(pixelUV);
                
                // 4. 输出颜色 (补全为 RGBA)
                return half4(sceneColor, 1.0);
            }
            ENDHLSL
        }
    }
}
