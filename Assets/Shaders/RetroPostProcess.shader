Shader "Mixtape/RetroPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorLevels ("Color Levels", Range(2, 256)) = 128
        _PixelGridStrength ("Pixel Grid Strength", Range(0, 1)) = 0.08
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.04
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            float _ColorLevels;
            float _PixelGridStrength;
            float _VignetteStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 16-bit color quantization: SNES/Genesis limited palette feel
                float levels = max(2, _ColorLevels);
                col.rgb = floor(col.rgb * levels + 0.5) / levels;
                col.rgb = saturate(col.rgb);

                // Subtle pixel grid: thin dark lines at pixel boundaries (not CRT scanlines)
                float2 pixelCoord = i.uv * _ScreenParams.xy;
                float2 grid = abs(frac(pixelCoord - 0.5) - 0.5);
                float gridLine = min(grid.x, grid.y) * 2.0;
                col.rgb *= 1.0 - _PixelGridStrength * (1.0 - saturate(gridLine));

                // Very subtle vignette
                float2 center = i.uv - 0.5;
                float vignette = 1.0 - dot(center, center) * 2.0 * _VignetteStrength;
                col.rgb *= saturate(vignette);

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
