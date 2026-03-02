Shader "Mixtape/UINoiseOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GrainIntensity ("Grain Intensity", Range(0, 0.2)) = 0.06
        _GrainSpeed ("Grain Speed", Float) = 3.0
        _GrainScale ("Grain Scale", Float) = 400.0
        _GrainBrightness ("Grain Brightness Bias", Range(-0.1, 0.1)) = 0
        _VignetteGrain ("Vignette Grain", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay+2" }
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
            float _GrainIntensity;
            float _GrainSpeed;
            float _GrainScale;
            float _GrainBrightness;
            float _VignetteGrain;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv * _GrainScale;
                float t = _Time.y * _GrainSpeed;

                // Two-layer noise for organic feel
                float n1 = frac(sin(dot(uv + t, float2(12.9898, 78.233))) * 43758.5453);
                float n2 = frac(sin(dot(uv * 1.3 + t * 0.7, float2(39.346, 11.135))) * 23421.631);
                float noise = (n1 * 0.6 + n2 * 0.4) - 0.5;

                // Stronger grain toward edges (vignette grain)
                float edgeDist = length(i.uv - 0.5) * 2.0;
                float vigBoost = 1.0 + edgeDist * edgeDist * _VignetteGrain;

                float grain = (noise * _GrainIntensity + _GrainBrightness) * vigBoost;
                return fixed4(grain, grain, grain, abs(grain));
            }
            ENDCG
        }
    }
}
