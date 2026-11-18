Shader "UI/TutorialSpotlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _DimColor ("Dim Color", Color) = (0, 0, 0, 0.75)
        _RimColor ("Rim Color", Color) = (1, 0.9, 0.4, 1)
        _Feather ("Feather (px)", Float) = 10
        _RimWidth ("Rim Width (px)", Float) = 6
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            fixed4 _DimColor;
            fixed4 _RimColor;
            float _Feather;
            float _RimWidth;
            float _DimAmount;   // 0~1 어두워지는 정도
            float _Pulse;       // 0~1 테두리 빛남
            float2 _Size;       // 캔버스 크기(px)
            float4 _Holes[8];   // xy = 최소점, zw = 최대점 (캔버스 좌하단 기준 px)
            float _HoleCount;
            float _RimCount;    // 앞에서부터 이 개수만큼만 테두리를 빛냄

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv * _Size;
                float dist = 100000.0;
                float rimDist = 100000.0;

                for (int k = 0; k < 8; k++)
                {
                    if (k >= _HoleCount) break;

                    float2 c = (_Holes[k].xy + _Holes[k].zw) * 0.5;
                    float2 h = (_Holes[k].zw - _Holes[k].xy) * 0.5;
                    float2 q = abs(p - c) - h;
                    float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);

                    dist = min(dist, d);
                    if (k < _RimCount) rimDist = min(rimDist, d);
                }

                float dim = _DimColor.a * _DimAmount * smoothstep(0.0, _Feather, dist);
                fixed4 col = fixed4(_DimColor.rgb, dim);

                float rim = (1.0 - smoothstep(0.0, _RimWidth, abs(rimDist))) * _Pulse * _DimAmount;
                col.rgb = lerp(col.rgb, _RimColor.rgb, rim);
                col.a = max(col.a, rim * _RimColor.a);
                return col;
            }
            ENDCG
        }
    }
}
