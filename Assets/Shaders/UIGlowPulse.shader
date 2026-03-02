Shader "Mixtape/UIGlowPulse"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 0.52, 0, 1)
        _GlowIntensity ("Intensity", Range(0, 1)) = 0.3
        _PulseSpeed ("Pulse Speed", Float) = 1.5
        _PulseAmount ("Pulse Amount", Range(0, 0.5)) = 0.15
        _Softness ("Softness", Range(0.5, 6)) = 2.0
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
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _PulseSpeed;
            float _PulseAmount;
            float _Softness;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 centered = i.uv - 0.5;
                float dist = length(centered);

                // Soft radial falloff — quadratic for natural light feel
                float glow = saturate(1.0 - dist * _Softness);
                glow = glow * glow;

                // Breathing pulse
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                float alpha = glow * _GlowIntensity * pulse;
                return fixed4(_GlowColor.rgb, alpha * _GlowColor.a);
            }
            ENDCG
        }
    }
}
