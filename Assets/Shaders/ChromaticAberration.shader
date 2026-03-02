Shader "Mixtape/ChromaticAberration"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 0.05)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay+10" }
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
            float _Intensity;

            fixed4 frag(v2f_img i) : SV_Target
            {
                if (_Intensity <= 0.0) return fixed4(0,0,0,0);

                float2 center = float2(0.5, 0.5);
                float2 dir = i.uv - center;

                // Sample R shifted outward, B shifted inward
                float r = tex2D(_MainTex, i.uv + dir * _Intensity).r;
                float g = tex2D(_MainTex, i.uv).g;
                float b = tex2D(_MainTex, i.uv - dir * _Intensity).b;

                // Vignette mask: effect strongest at edges
                float vignette = saturate(dot(dir, dir) * 4.0);

                float alpha = vignette * (_Intensity / 0.05) * 0.85;

                return fixed4(r, g, b, alpha);
            }
            ENDCG
        }
    }
}
