Shader "Hidden/PixelizeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Float) = 10
    }
    SubShader
    {
        //这是后处理的标准设置：无剔除、无深度测试
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _PixelSize; // 像素块的大小

            fixed4 frag (v2f i) : SV_Target
            {
                // 获取屏幕分辨率
                float2 screenRes = _ScreenParams.xy;
                
                // 计算当前屏幕应该划分为多少个像素块
                // 使用_PixelSize来控制块的大小，保证像素是正方形的
                float2 blockCount = screenRes / _PixelSize;

                // 核心算法：
                // 1. 将UV坐标乘以前块的数量
                // 2. 向下取整(floor)，实现阶梯状的数据
                // 3. 再除以块的数量，还原回UV的0-1范围
                float2 pixelUV = floor(i.uv * blockCount) / blockCount;

                // 使用处理后的UV采样纹理
                return tex2D(_MainTex, pixelUV);
            }
            ENDCG
        }
    }
}