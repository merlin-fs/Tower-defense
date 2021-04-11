Shader "Fade/FillAmount2Color"
{
    Properties
    {
        [MainColor] _Color("Tint", Color) = (1,1,1,1)
        _Color2("Second Tint", Color) = (1,1,1,1)
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _TransitionTex("Transition Texture", 2D) = "white" {}
        //[PerRendererData] 
        _Cutoff("Cutoff", Range(0, 1)) = 0
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Overlay"
                "RenderType" = "Opaque"
                "ForceNoShadowCasting" = "True"
                "IgnoreProjector" = "True"
                "CanUseSpriteAtlas" = "True"
            }
            LOD 100

            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
            CGPROGRAM
                //#pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag
                //#pragma multi_compile_fog
                #pragma multi_compile_instancing

                #pragma instancing_options assumeuniformscaling
                #pragma instancing_options nolodfade
                #pragma instancing_options nolightprobe
                #pragma instancing_options nolightmap
                #include "UnityCG.cginc"

                struct appdata
                {
                    UNITY_VERTEX_INPUT_INSTANCE_ID

                    float2 uv       : TEXCOORD0;
                    float4 vertex   : POSITION;
                };

                struct v2f
                {
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    
                    float2 uv       : TEXCOORD0;
                    float4 vertex   : SV_POSITION;
                };

                sampler2D _MainTex;
                sampler2D _TransitionTex;
                fixed4 _Color;
                fixed4 _Color2;

                UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
                UNITY_INSTANCING_BUFFER_END(Props)

                v2f vert(appdata _in)
                {
                    v2f _out;
                    UNITY_SETUP_INSTANCE_ID(_in);
                    UNITY_TRANSFER_INSTANCE_ID(_in, _out);
                    _out.vertex = UnityObjectToClipPos(_in.vertex);
                    _out.uv = _in.uv;
                    return _out;
                }

                fixed4 frag(v2f _in) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(_in);

                    float cutoff = UNITY_ACCESS_INSTANCED_PROP(Props, _Cutoff);
                    fixed4 _out = tex2D(_MainTex, _in.uv);
                    fixed4 transit = tex2D(_TransitionTex, _in.uv);

                    bool cut = transit.b > cutoff;
                    _out.rgb = cut ? _out.rgb * _Color2 : _out.rgb * _Color;
                    _out.a = cut ? _out.a * _Color2.a : _out.a * _Color.a;
                    return _out;
                }
            ENDCG
            }
        }
}