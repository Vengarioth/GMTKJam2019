Shader "Unlit/obstacle_shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FxTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.2, 0.2, 0.3, 1.)
        _SecondaryColor ("Secondary Color", Color) = (1., 1., 1., 1.)
        _Scale ("Scale", Float) = 1.0
        _TimeScale ("TimeScale", Float) = 1.0
        _ColorClamp ("Color Clamp", Float) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
        Blend SrcAlpha OneMinusSrcAlpha
        
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members uv1,uv2)
#pragma exclude_renderers d3d11
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
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _FxTex;
            float4 _MainTex_ST;
            float4 _FxTex_ST;
            
            fixed4 _BaseColor;
            fixed4 _SecondaryColor;
            float _Scale;
            float _TimeScale;
            float _ColorClamp;

            v2f vert (appdata v)
            {
                v2f o;
                
                float4 pos = UnityObjectToClipPos(v.vertex);
                
                float t = _Time.x * 0.05 * _TimeScale;
                
                o.vertex = pos;
                pos *= 0.3 * _Scale;
                pos.xy += t;
                pos.x *= 1.6;
                o.uv0 = float2(pos.x, pos.y) * 3;//TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = o.uv0 * 2.777 + t * -20;
                
                o.uv2 = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col0 = tex2D(_FxTex, i.uv0);
                fixed4 col1 = tex2D(_FxTex, i.uv1);
                fixed4 col2 = tex2D(_MainTex, i.uv2);
                
                float t = _Time.x * 5;
                float thr = 0.4 + sin(t) * 0.1;
                col0 = smoothstep(thr, thr + 0.2, col0 * col1);
                col1 = col1;
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col0);
                
                col0 = min(col0, _ColorClamp);
                fixed4 fin = col0;
                fin = lerp(_BaseColor, _SecondaryColor, col0);
                fin.a = col2.a;
                return fin;
            }
            ENDCG
        }
    }
}
