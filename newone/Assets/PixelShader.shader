Shader "Custom/PixelEffectOptimized"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _PixelColumns ("Pixel Columns (Width)", Float) = 512
        _PixelRows ("Pixel Rows (Height)", Float) = 288
        _PixelSmoothness ("Pixel Smoothness", Range(0,1)) = 0
        _ScreenResolution ("Screen Resolution", Vector) = (1920,1080,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" "IgnoreProjector"="True" }
        LOD 100
        ZWrite Off ZTest Always Cull Off // 后处理Shader必备，关闭深度写入/剔除

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PixelColumns;
            float _PixelRows;
            float _PixelSmoothness;
            float2 _ScreenResolution;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 核心像素化采样：计算像素块中心UV，获取像素化颜色
                float2 pixelCount = float2(_PixelColumns, _PixelRows);
                // 计算当前UV对应的像素块索引
                float2 pixelBlockIndex = floor(i.uv * pixelCount);
                // 计算像素块中心UV，避免采样偏移
                float2 pixelatedUV = (pixelBlockIndex + 0.5) / pixelCount;
                // 采样像素化颜色
                fixed4 pixelatedColor = tex2D(_MainTex, pixelatedUV);

                // 2. 采样原始清晰颜色
                fixed4 originalColor = tex2D(_MainTex, i.uv);

                // 3. 按平滑度混合两种颜色，实现精细过渡
                // _PixelSmoothness=0 → 完全像素化；_PixelSmoothness=1 → 完全原图；0-1之间渐变
                fixed4 finalColor = lerp(pixelatedColor, originalColor, _PixelSmoothness);

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
