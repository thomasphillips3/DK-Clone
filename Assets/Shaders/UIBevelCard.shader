Shader "Mixtape/UIBevelCard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.09, 0.09, 0.11, 1)
        _CornerRadius ("Corner Radius", Range(0, 0.2)) = 0.05
        _BevelStrength ("Bevel Strength", Range(0, 0.4)) = 0.12
        _ShadowInset ("Shadow Inset", Range(0.01, 0.15)) = 0.04
        _NoiseIntensity ("Noise", Range(0, 0.1)) = 0.02
        _HighlightStrength ("Top Highlight", Range(0, 0.3)) = 0.08
        _DepressAmount ("Depress Darken", Range(0, 0.15)) = 0
        _InnerGlowColor ("Inner Glow Color", Color) = (0, 0, 0, 0)
        _InnerGlowWidth ("Inner Glow Width", Range(0, 0.1)) = 0
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
            fixed4 _BaseColor;
            float _CornerRadius;
            float _BevelStrength;
            float _ShadowInset;
            float _NoiseIntensity;
            float _HighlightStrength;
            float _DepressAmount;
            fixed4 _InnerGlowColor;
            float _InnerGlowWidth;

            fixed4 frag(v2f_img i) : SV_Target
            {
                // SDF rounded rectangle
                float2 halfSize = float2(0.5, 0.5) - _CornerRadius;
                float2 d = abs(i.uv - 0.5) - halfSize;
                float sdf = length(max(d, 0.0)) - _CornerRadius;

                // Anti-aliased edge
                float alpha = saturate(-sdf / fwidth(sdf));

                // Inner bevel: darken near edges for depth
                float edgeX = min(i.uv.x, 1.0 - i.uv.x);
                float edgeY = min(i.uv.y, 1.0 - i.uv.y);
                float edgeDist = saturate(min(edgeX, edgeY) / _ShadowInset);
                float bevel = lerp(1.0 - _BevelStrength, 1.0, edgeDist);

                // Subtle top-edge highlight (light from above)
                float highlight = (1.0 - i.uv.y) * _HighlightStrength * edgeDist;

                // Procedural noise grain
                float noise = frac(sin(dot(i.uv * 200.0, float2(12.9898, 78.233))) * 43758.5453);
                noise = (noise - 0.5) * _NoiseIntensity;

                float3 col = _BaseColor.rgb * bevel + highlight + noise;

                // Inner glow ring (when _InnerGlowWidth > 0)
                if (_InnerGlowWidth > 0.001)
                {
                    float innerRingDist = abs(sdf + _InnerGlowWidth);
                    float innerRing = saturate(1.0 - innerRingDist / _InnerGlowWidth);
                    innerRing = innerRing * innerRing;
                    col += _InnerGlowColor.rgb * innerRing * _InnerGlowColor.a;
                }

                // Depress darkening
                col *= (1.0 - _DepressAmount);

                return fixed4(col, alpha * _BaseColor.a);
            }
            ENDCG
        }
    }
}
