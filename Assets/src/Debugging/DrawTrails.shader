﻿Shader "Unlit/DrawTrails"
{
    Properties
    {
		_MainTex("Texture", 2D) = "black" {}
		_BackTex("Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			sampler2D _BackTex;
			float4 _BackTex_ST;

			float _PaintTime;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float r1 = tex2D(_MainTex, i.uv).r;
				float r2 = tex2D(_BackTex, i.uv).r;

				float r = max(r1, r2);

				r1 = r1 <= 0.0 ? 0.0 : r1 <= _PaintTime ? 1.0 : 0.25;
				r2 = r2 <= 0.0 ? 0.0 : r2 <= _PaintTime ? 1.0 : 0.25;

                // sample the texture
                fixed4 col = fixed4(r1, r2, 0.0, r >= 0.1 ? 1.0 : 0.0);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
