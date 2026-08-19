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
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float3 worldPos : TEXCOORD0; };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Simple directional lighting approximation using world position
                // Gives a subtle gradient so the geometry looks 3D
                fixed3 lightDir = normalize(fixed3(0.5, 1, -0.3));
                fixed3 normal = normalize(cross(ddx(i.worldPos), ddy(i.worldPos)));
                fixed ndotl = max(0, dot(normal, lightDir));
                return fixed4(_Color.rgb * (0.3 + 0.7 * ndotl), _Color.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}