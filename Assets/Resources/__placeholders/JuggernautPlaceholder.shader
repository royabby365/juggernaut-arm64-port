Shader "Hidden/JuggernautPlaceholder"
{
    Properties
    {
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            precision highp float;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Lambertian diffuse lighting — full float precision
                float3 lightDir = normalize(float3(0.5, 1, -0.3));
                float3 worldN = normalize(i.worldNormal);
                float ndotl = saturate(dot(worldN, lightDir));

                // Full dynamic range: 10% ambient + 90% diffuse
                float3 lit = _Color.rgb * (0.10 + 0.90 * ndotl);

                // Distance fog
                float3 viewDir = _WorldSpaceCameraPos - i.worldPos;
                float dist = length(viewDir);
                float fog = saturate(dist * 0.03);
                lit = lerp(lit, unity_FogColor.rgb, fog);

                // Screen-space dither to break up banding on 16-bit framebuffers
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float dither = frac(sin(dot(screenUV * _ScreenParams.xy, float2(12.9898, 78.233))) * 43758.5453);
                dither = (dither - 0.5) / 255.0;
                lit += dither;

                return float4(lit, _Color.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}