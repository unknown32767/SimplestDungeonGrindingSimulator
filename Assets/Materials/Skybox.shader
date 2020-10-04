Shader "Unlit/Skybox"
{
    Properties
    {
        _Threshold ("Threshold", float) = 0.999
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float rand(float2 uv)
            {
                return frac(sin(dot(uv.xy, float2(114.5141, 19.19810))) * 43758.5453);
            }

            float _Threshold;

            fixed4 frag (v2f i) : SV_Target
            {
                float random = rand(normalize(i.uv));
                return fixed4((random-_Threshold)/(1-_Threshold) * fixed3(1,1,1), 1);
            }
            ENDHLSL
        }
    }
}
