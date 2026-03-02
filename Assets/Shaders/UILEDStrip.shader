Shader "Mixtape/UILEDStrip"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StripColor ("Strip Color", Color) = (1, 0.52, 0, 1)
        _Progress ("Progress", Range(0, 1)) = 0.5
        _StripHeight ("Strip Height", Range(0.01, 0.3)) = 0.08
        _StripY ("Strip Y Position", Range(0, 1)) = 0.0
        _Glow ("Glow Spread", Range(0, 0.3)) = 0.06
        _PulseSpeed ("Pulse Speed", Float) = 0
        _PulseAmount ("Pulse Amount", Range(0, 0.5)) = 0
        // Ring mode
        _RingMode ("Ring Mode", Float) = 0
        _RingWidth ("Ring Width", Range(0.01, 0.15)) = 0.04
        _RingCornerRadius ("Ring Corner Radius", Range(0, 0.2)) = 0.05
        // Sweep mode
        _SweepMode ("Sweep Mode", Float) = 0
        _SweepProgress ("Sweep Position", Range(-0.3, 1.3)) = -0.3
        _SweepWidth ("Sweep Width", Range(0.02, 0.2)) = 0.08
        _SweepColor ("Sweep Color", Color) = (1, 1, 1, 0.06)
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
            fixed4 _StripColor;
            float _Progress;
            float _StripHeight;
            float _StripY;
            float _Glow;
            float _PulseSpeed;
            float _PulseAmount;
            float _RingMode;
            float _RingWidth;
            float _RingCornerRadius;
            float _SweepMode;
            float _SweepProgress;
            float _SweepWidth;
            fixed4 _SweepColor;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float alpha = 0.0;
                float3 col = float3(0, 0, 0);

                if (_RingMode > 0.5)
                {
                    // --- Ring Mode: SDF rounded-rect inner glow ---
                    float2 halfSize = float2(0.5, 0.5) - _RingCornerRadius;
                    float2 d = abs(uv - 0.5) - halfSize;
                    float sdf = length(max(d, 0.0)) - _RingCornerRadius;

                    // Inner ring glow
                    float ringDist = abs(sdf + _RingWidth * 0.5);
                    float ring = saturate(1.0 - ringDist / (_RingWidth * 0.5));
                    ring = ring * ring;

                    // Only inside the rect
                    float inside = saturate(-sdf / fwidth(sdf));
                    ring *= inside;

                    float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                    alpha = ring * _StripColor.a * pulse;
                    col = _StripColor.rgb;
                }
                else
                {
                    // --- Strip Mode: Horizontal LED bar ---
                    float distFromStrip = abs(uv.y - _StripY) - _StripHeight * 0.5;
                    float stripMask = saturate(-distFromStrip / max(fwidth(uv.y), 0.001));

                    // Glow falloff beyond strip edges
                    float glowMask = saturate(1.0 - max(distFromStrip, 0.0) / max(_Glow, 0.001));
                    glowMask = glowMask * glowMask;

                    // Progress mask
                    float progressMask = saturate((_Progress - uv.x) / max(fwidth(uv.x), 0.001));

                    float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                    float combined = max(stripMask, glowMask * 0.4) * progressMask * pulse;

                    alpha = combined * _StripColor.a;
                    col = _StripColor.rgb;
                }

                // --- Sweep overlay (additive on both modes) ---
                if (_SweepMode > 0.5)
                {
                    // Diagonal sweep line
                    float sweepPos = uv.x * 0.7 + uv.y * 0.3;
                    float sweepDist = abs(sweepPos - _SweepProgress);
                    float sweep = saturate(1.0 - sweepDist / _SweepWidth);
                    sweep = sweep * sweep;
                    col += _SweepColor.rgb * sweep * _SweepColor.a;
                    alpha = max(alpha, sweep * _SweepColor.a);
                }

                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
}
