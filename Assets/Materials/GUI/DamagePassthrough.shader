Shader "UI/VignetteEffect"
{
    Properties
    {
        _VignetteColor("Vignette Color", Color) = (1, 0, 0, 1)
        _Intensity("Intensity", Range(0, 1)) = 0.5
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
        _Padding("Padding", Range(0, 5)) = 0.3
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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

            float4 _VignetteColor;
            float _Intensity;
            float _Smoothness;
            float _Padding;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv * 2.0 - 1.0; // Transform UV range from [0,1] to [-1,1]
                float dist = length(uv) * (1 + _Padding);
                float vignette = smoothstep(0, 1, clamp(0, 1, (dist * sqrt(_Intensity)) - _Smoothness));
                float4 color = _VignetteColor;
                color.a *= vignette;
                return color;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
