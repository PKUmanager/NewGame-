// 定义Shader的路径（材质面板里能找到的位置）
Shader "Custom/PixelationImageEffect" {
    // 暴露给材质面板的可编辑参数
    Properties {
        _MainTex ("Screen Texture", 2D) = "white" {} // 接收相机的屏幕纹理（固定参数，不用改）
        _PixelColumns ("像素列数", Int) = 100 // 第一个参数：像素列数（默认100）
        _PixelRows ("像素行数", Int) = 100   // 第二个参数：像素行数（默认100）
    }

    SubShader {
        // 渲染标签：后处理专用，确保最后渲染
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        // 后处理Shader核心配置：关闭裁剪/深度写入/深度测试
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            // 声明顶点/片元着色器函数
            #pragma vertex vert
            #pragma fragment frag
            // 引入Unity内置工具宏
            #include "UnityCG.cginc"

            // ========== 第一步：定义参数变量（与Properties对应） ==========
            sampler2D _MainTex;       // 屏幕纹理
            int _PixelColumns;        // 像素列数（接收材质面板的参数）
            int _PixelRows;           // 像素行数（接收材质面板的参数）

            // ========== 顶点输入结构体（固定模板，不用改） ==========
            struct appdata {
                float4 vertex : POSITION; // 顶点位置
                float2 uv : TEXCOORD0;    // 纹理UV坐标
            };

            // ========== 顶点输出/片元输入结构体（固定模板，不用改） ==========
            struct v2f {
                float2 uv : TEXCOORD0;    // 传递给片元的UV坐标
                float4 vertex : SV_POSITION; // 裁剪空间顶点位置
            };

            // ========== 顶点着色器（固定逻辑，不用改） ==========
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 顶点坐标转换
                o.uv = v.uv; // 直接传递UV坐标给片元着色器
                return o;
            }

            // ========== 片元着色器（核心逻辑：像素化） ==========
            fixed4 frag (v2f i) : SV_Target {
                // ========== 第二步：调用行列数参数，计算像素块坐标 ==========
                // 1. 计算每个像素块的UV步长（把0-1的UV范围分成行列数个块）
                float2 pixelStep = float2(1.0 / _PixelColumns, 1.0 / _PixelRows);
                // 2. 将当前UV坐标转换为「像素块索引」，并取整（找到最近的整数坐标块）
                float2 pixelIndex = floor(i.uv / pixelStep); // floor=向下取整（取最近的整数）
                // 3. 计算该整数索引对应的UV坐标（即块的中心/左上角坐标）
                float2 pixelUV = pixelIndex * pixelStep;

                // ========== 第三步：修改像素颜色 ==========
                // 采样「整数坐标块」的像素颜色，作为当前像素的颜色
                fixed4 col = tex2D(_MainTex, pixelUV);
                return col; // 输出最终颜色
            }
            ENDCG
        }
    }
    // 无降级方案（后处理Shader无需降级）
    FallBack Off
}