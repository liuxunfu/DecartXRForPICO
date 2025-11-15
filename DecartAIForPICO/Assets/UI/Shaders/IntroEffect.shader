Shader "Unlit/IntroEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CustomTime ("Custom Time", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-1" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Utils.cginc"

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
                float3 objectPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CustomTime;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.objectPos = v.vertex.xyz;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // Pre-calculated constants for optimization
            static const float3x3 GOLD = float3x3(
                -0.571464913, +0.814921382, +0.096597072,
                -0.278044873, -0.303026659, +0.911518454,
                +0.772087367, +0.494042493, +0.399753815);

            float4 sampleTextureRGBA(float x, float y, float z)
            {
                float t = _CustomTime - 0.5;
                // Calculate noise for each channel with proper Y and Z offsets
                float colorOffset = 0.01;

                // Red channel: y-colorOffset, z-0.02
                float2 posR = float2(x, z-0.02) * 8;
                float noiseR = dot_noise(float3(posR*5, t * 2)) * 0.3;
                noiseR = 0.8 - abs(noiseR);
                float2 posR_rot = mul(GOLD, posR);
                noiseR -= abs(dot_noise(float3(posR_rot*7, t * 3)) * 0.1);
                float causticR = saturate(-sin(t*3*1.5 + (y-colorOffset)*8 + noiseR));
                causticR = causticR * causticR * causticR;
                causticR = causticR * causticR;

                // Green channel: y, z (original position)
                float2 posG = float2(x, z) * 8;
                float noiseG = dot_noise(float3(posG*5, t * 2)) * 0.3;
                noiseG = 0.8 - abs(noiseG);
                float2 posG_rot = mul(GOLD, posG);
                noiseG -= abs(dot_noise(float3(posG_rot*7, t * 3)) * 0.1);
                float causticG = saturate(-sin(t*3*1.5 + y*8 + noiseG));
                causticG = causticG * causticG * causticG;
                causticG = causticG * causticG;

                // Blue channel: y+colorOffset, z+0.02
                float2 posB = float2(x, z+0.02) * 8;
                float noiseB = dot_noise(float3(posB*5, t * 2)) * 0.3;
                noiseB = 0.8 - abs(noiseB);
                float2 posB_rot = mul(GOLD, posB);
                noiseB -= abs(dot_noise(float3(posB_rot*7, t * 3)) * 0.1);
                float causticB = saturate(-sin(t*3*1.5 + (y+colorOffset)*8 + noiseB));
                causticB = causticB * causticB * causticB;
                causticB = causticB * causticB;

                // Alpha channel: same as green (original position)
                float causticA = causticG;

                return float4(causticR, causticG, causticB, causticA);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // show UV values as colors (R=U, G=V) and object position as blue
                float x = (i.objectPos.x + 1.0) * 0.5;
                float y = (i.objectPos.y + 1.0) * 0.5;
                float z = (i.objectPos.z + 1.0) * 0.5;

                // Use optimized function that calculates all channels at once
                float4 rgba = sampleTextureRGBA(x, y, z);

                // Replace pow(a, 0.5) with sqrt() for better performance
                fixed4 color = fixed4(rgba.r, rgba.g, rgba.b, sqrt(rgba.a));

                return color;
            }
            ENDCG
        }
    }
}
