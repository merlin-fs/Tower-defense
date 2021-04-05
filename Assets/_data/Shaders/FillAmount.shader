Shader "Fade/FillAmount"
{
    Properties
    {
        _Color("Tint", Color) = (1,1,1,1)
        _MainTex("Sprite Texture", 2D) = "white" {}
        _TransitionTex("Transition Texture", 2D) = "white" {}
        _Cutoff("Cutoff", Range(0, 1)) = 0
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0

    }
        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Opaque"
            }
            LOD 300
            Cull Off
            Lighting Off
            ZWrite Off
            //Blend One OneMinusSrcAlpha
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
                #pragma surface surf Ramp alpha:fade

              half4 LightingRamp(SurfaceOutput s, half3 lightDir, half atten) 
              {
                  return half4(s.Albedo.r, s.Albedo.g, s.Albedo.b, s.Alpha);
              }

                sampler2D _MainTex;
                sampler2D _TransitionTex;
                float _Cutoff;
                fixed4 _Color;

                struct Input
                {
                    float2 uv_MainTex;
                };

                void surf(Input IN, inout SurfaceOutput o) {

                    fixed4 diffuse = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                    fixed4 fadeSample = tex2D(_TransitionTex, IN.uv_MainTex);

                    bool cut = (fadeSample.r + fadeSample.g + fadeSample.b) / 3.0 < _Cutoff ? false : true;
                    o.Albedo = diffuse.rgb;
                    o.Alpha = cut ? 0 : 1;
                }
            ENDCG
        }
        FallBack "Transparent/Cutout/Diffuse"
}