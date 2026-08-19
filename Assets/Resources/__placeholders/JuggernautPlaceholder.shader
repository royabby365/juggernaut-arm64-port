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
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Lambertian diffuse lighting using REAL vertex normals
                fixed3 lightDir = normalize(fixed3(0.5, 1, -0.3));
                fixed3 normal = normalize(i.worldNormal);
                fixed ndotl = max(0, dot(normal, lightDir));

                // Ambient (25%) + diffuse (75%) — backfaces rendered at 25% not 0%
                fixed3 lit = _Color.rgb * (0.25 + 0.75 * ndotl);

                // Distance fog — fades toward unity_FogColor (set in Lighting window)
                float3 viewDir = _WorldSpaceCameraPos - i.worldPos;
                float dist = length(viewDir);
                float fog = saturate(dist * 0.03);
                lit = lerp(lit, unity_FogColor.rgb, fog);

                return fixed4(lit, _Color.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}