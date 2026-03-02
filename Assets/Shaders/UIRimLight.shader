Shader "Mixtape/UIRimLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RimColor ("Rim Color", Color) = (1, 0.52, 0, 1)
        _RimWidth ("Rim Width", Range(0.005, 0.1)) = 0.02
        _RimSoftness ("Rim Softness", Range(0.5, 4)) = 1.5
        _RimIntensity ("Rim Intensity", Range(0, 1)) = 0.5
        _InnerAlpha ("Inner Alpha", Range(0, 1)) = 0
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
            fixed4 _RimColor;
            float _RimWidth;
            float _RimSoftness;
            float _RimIntensity;
            float _InnerAlpha;

            fixed4 frag(v2f_img i) : SV_Target
            {
                // Edge distance from rect boundary
                float edgeX = min(i.uv.x, 1.0 - i.uv.x);
                float edgeY = min(i.uv.y, 1.0 - i.uv.y);
                float edgeDist = min(edgeX, edgeY);

                // Rim glow at edges
                float rim = saturate(1.0 - edgeDist / _RimWidth);
                rim = pow(rim, _RimSoftness);

                // Interior fill
                float interior = saturate(edgeDist / _RimWidth);
                float interiorAlpha = interior * _InnerAlpha;

                float finalAlpha = max(rim * _RimIntensity, interiorAlpha) * _RimColor.a;

                return fixed4(_RimColor.rgb, finalAlpha);
            }
            ENDCG
        }
    }
}
