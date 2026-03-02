Shader "Mixtape/UIGradientVignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // Gradient mode
        _TopColor ("Top Color", Color) = (0.04, 0.04, 0.10, 1)
        _BottomColor ("Bottom Color", Color) = (0.06, 0.05, 0.04, 1)
        _GradientBias ("Gradient Bias", Range(0, 1)) = 0.4
        // Vignette
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.6
        _VignetteRadius ("Vignette Radius", Range(0.2, 1.5)) = 0.8
        // Bloom
        _BloomColor ("Bloom Color", Color) = (1, 0.52, 0, 0.15)
        _BloomSpeed ("Bloom Speed", Float) = 0.3
        _BloomAmount ("Bloom Amount", Range(0, 0.3)) = 0.08
        _BloomCenterX ("Bloom Center X", Float) = 0.5
        _BloomCenterY ("Bloom Center Y", Float) = 0.6
        _BloomSoftness ("Bloom Softness", Range(0.5, 4)) = 2.0
        // Glass mode
        _GlassMode ("Glass Mode", Float) = 0
        _GlassOpacity ("Glass Opacity", Range(0, 1)) = 0.08
        _GlassTint ("Glass Tint", Color) = (1, 0.92, 0.8, 1)
        _HighlightPos ("Highlight Position", Range(0, 1)) = 0.15
        _HighlightWidth ("Highlight Width", Range(0.01, 0.2)) = 0.04
        _GlassHighlight ("Glass Highlight Strength", Range(0, 0.5)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _GradientBias;
            float _VignetteStrength;
            float _VignetteRadius;
            fixed4 _BloomColor;
            float _BloomSpeed;
            float _BloomAmount;
            float _BloomCenterX;
            float _BloomCenterY;
            float _BloomSoftness;
            float _GlassMode;
            float _GlassOpacity;
            fixed4 _GlassTint;
            float _HighlightPos;
            float _HighlightWidth;
            float _GlassHighlight;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;

                if (_GlassMode > 0.5)
                {
                    // --- Glass Panel Mode ---
                    // Frosted glass base
                    float3 col = _GlassTint.rgb * _GlassOpacity;

                    // Horizontal highlight band near top
                    float highlightDist = abs(uv.y - (1.0 - _HighlightPos));
                    float highlight = saturate(1.0 - highlightDist / _HighlightWidth);
                    highlight = highlight * highlight * _GlassHighlight;
                    col += highlight;

                    // Subtle inner vignette for depth
                    float2 vigUV = uv - 0.5;
                    float vigDist = length(vigUV);
                    float vig = saturate(vigDist / 0.7);
                    col *= lerp(1.0, 0.85, vig * vig);

                    // Faint noise for frosted texture
                    float noise = frac(sin(dot(uv * 300.0, float2(12.9898, 78.233))) * 43758.5453);
                    col += (noise - 0.5) * 0.015;

                    return fixed4(col, _GlassTint.a * _GlassOpacity + highlight * 0.5);
                }
                else
                {
                    // --- Gradient Background Mode ---
                    // Biased vertical gradient
                    float gradT = saturate(pow(uv.y, lerp(0.5, 2.0, _GradientBias)));
                    float3 col = lerp(_BottomColor.rgb, _TopColor.rgb, gradT);

                    // Radial vignette
                    float2 vigUV = uv - 0.5;
                    float vigDist = length(vigUV);
                    float vig = saturate((vigDist - _VignetteRadius * 0.5) / (_VignetteRadius * 0.5));
                    vig = vig * vig;
                    col *= lerp(1.0, 1.0 - _VignetteStrength, vig);

                    // Breathing bloom
                    float2 bloomCenter = float2(_BloomCenterX, _BloomCenterY);
                    float bloomDist = length(uv - bloomCenter);
                    float bloom = saturate(1.0 - bloomDist * _BloomSoftness);
                    bloom = bloom * bloom;
                    float pulse = 1.0 + sin(_Time.y * _BloomSpeed) * _BloomAmount;
                    col += _BloomColor.rgb * bloom * _BloomColor.a * pulse;

                    return fixed4(col, 1.0);
                }
            }
            ENDCG
        }
    }
}
