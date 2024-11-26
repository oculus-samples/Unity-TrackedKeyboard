// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

Shader "Custom/Area"
{
    Properties
    {
        [MainTexture] _MainTex ("Icon", 2D) = "black" {}
         _IconColor ("Icon Color", Color) = (1, 1, 1, 1)
        _IconParams ("Icon Size (x) and Position (y,z,w)", Vector) = (1,1,1,1)
         _TintColor ("Color", Color) = (1, 1, 1, 1)
         _FrameColor ("Frame Color", Color) = (1, 1, 1, 1)
        _OutlineRadius ("Radius", Range(0, 1)) = 1
        _OutlineSize ("Stroke", Range(-0.1, 10)) = 0.05
        _Alpha ("Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Blend SrcAlpha OneMinusSrcAlpha, One One
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldScale : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
              float4 _MainTex_ST;
              float4 _IconColor;
              float4 _IconParams;
              float4 _TintColor;
              float4 _FrameColor;
              float  _OutlineSize;
              float _OutlineRadius;
              float _Alpha;

            float sdRoundBox(float2 p, float2 size, float radius)
            {
                float2 d = abs(p) - size * 0.5 + radius;
                return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - radius;
            }

            // anti-aliasing step
            float aaStep(float gradient)
            {
                float width = fwidth(gradient);
                return saturate((gradient - (0.5 - width)) / (2.0 * width));
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 worldScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                o.worldScale = worldScale.xz;
                o.uv = v.vertex.xz * worldScale.xz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float box = sdRoundBox(i.uv, i.worldScale - 0.001, _OutlineRadius * 0.1);
                float body = aaStep(1.0 - smoothstep(0, 0.001, box));
                float frame = 1.0 - smoothstep(0, 0.001, abs(box + _OutlineSize * 0.01) - _OutlineSize * 0.01);
                float4 col = lerp(_TintColor, _FrameColor, aaStep(frame));

                float2 iconUV = float2(
                    (i.uv.x - (i.worldScale.x * 0.5 - _IconParams.y)) * (1.0 / _IconParams.x),
                    (i.uv.y + (i.worldScale.y * 0.5 - _IconParams.z)) * (1.0 / _IconParams.x)
                );

                fixed4 icon = tex2D(_MainTex, iconUV);
                col = lerp(col, _IconColor, icon.a);

                return float4(col.rgb, saturate(col.a * body * _Alpha));
            }
            ENDCG
        }
    }
}