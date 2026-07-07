Shader "Sprite Overlay Dissolve"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Overlay("Overlay", Color) = (0, 0, 0 ,0)
        _Noise ("Disolve Texture", 2D) = "white" {}
        _Disolve ("Disolve", Range(0, 1)) = 0
        _Outline ("Outline", Range(0, 0.1)) = 0.03
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 2.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color: COLOR;
                float4 vertex : SV_POSITION;
                float2 positionWS: TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.positionWS = mul(unity_ObjectToWorld, v.vertex).xy;
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            UNITY_INSTANCING_BUFFER_START(Props)

            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Overlay)

            UNITY_DEFINE_INSTANCED_PROP(float, _Disolve)
            UNITY_DEFINE_INSTANCED_PROP(float, _Outline)

            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;
            sampler2D _Noise;

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                fixed4 col = tex2D (_MainTex, i.uv) * i.color * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                fixed4 overlay = UNITY_ACCESS_INSTANCED_PROP(Props, _Overlay);
                col.rgb = lerp(col.rgb, overlay.rgb, overlay.a);

                float noise = tex2D (_Noise, i.positionWS).x;
                float dif = UNITY_ACCESS_INSTANCED_PROP(Props, _Disolve) - noise;

                if(dif > UNITY_ACCESS_INSTANCED_PROP(Props, _Outline))
                {
                    col.a = 0;
                } else if(dif > 0)
                {
                    col.rgb = 0;
                }

                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}
