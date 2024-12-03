// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

Shader "TrackedKeyboard/KeyboardHeightFade" {
  Properties {
    _TintColor      ("Color", Color) = (1, 1, 1, 1)
    _Alpha          ("Alpha", Range(0,1)) = 1
  }

  SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100

    ZWrite Off
    Cull Off
    Blend SrcAlpha OneMinusSrcAlpha, One One

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 2.0

      #include "UnityCG.cginc"

      struct appdata_t {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      float4 _TintColor;
      float _Alpha;

      v2f vert (appdata_t v)
      {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }

      fixed4 frag (v2f i) : SV_Target
      {
        fixed4 col = _TintColor;
        col.a *= (1 - i.uv.y) * _Alpha;
        return col;
      }
      ENDCG
    }
  }

}
