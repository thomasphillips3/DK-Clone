Shader "Mixtape/VHSWipe"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0, 1)) = 0
        _Color ("Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay+1" }
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
            float _Progress;
            fixed4 _Color;

            fixed4 frag(v2f_img i) : SV_Target
            {
                if (_Progress <= 0.0) return fixed4(0,0,0,0);

                // Per-line noise for VHS scan displacement
                float noise = frac(sin(dot(float2(i.uv.x, floor(i.uv.y * 240.0)) + _Time.y * 3.7,
                    float2(127.1, 311.7))) * 43758.5453123);

                float lineNoise = (noise - 0.5) * 0.03;

                // Wipe edge with noise
                float edge = i.uv.y - _Progress + lineNoise;
                float alpha = step(edge, 0.0);

                if (alpha < 0.5) return fixed4(0,0,0,0);

                // Scan line darkening
                float scan = fmod(i.uv.y * 240.0, 1.0) < 0.5 ? 0.82 : 1.0;

                return fixed4(_Color.r, _Color.g, _Color.b, _Color.a * scan);
            }
            ENDCG
        }
    }
}
