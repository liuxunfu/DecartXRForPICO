Shader "Unlit/UIParallax"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ParallaxStrength ("Parallax Strength", Float) = 0.02
        _ParallaxDepth ("Parallax Depth", Float) = 1.0
        _RectSize ("Rect Size", Vector) = (1,1,0,0)
        _Radius ("Corner Radius", Vector) = (0,0,0,0)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "Utils.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 tangent  : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4  mask : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float2 rectSize : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            float _ParallaxStrength;
            float _ParallaxDepth;
            float4 _RectSize;
            float4 _Radius;

            // Pre-calculated constants for optimization
            static const float3x3 GOLD = float3x3(
                -0.571464913, +0.814921382, +0.096597072,
                -0.278044873, -0.303026659, +0.911518454,
                +0.772087367, +0.494042493, +0.399753815);

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;
                
                // Calculate view direction in object space for UI parallax
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                OUT.viewDir = mul(unity_WorldToObject, float4(worldViewDir, 0)).xyz;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));
                OUT.rectSize = clampedRect.zw - clampedRect.xy;

                OUT.color = v.color * _Color;
                return OUT;
            }

            half4 sampleTexture(sampler2D tex, float2 uv)
            {
                uv = uv * 2 - 1; // Transform UVs from [0,1] to [-1,1]
                uv *= 0.7; // Zoom into center
                uv = uv * 0.5 + 0.5; // Transform UVs back to [0,1]
                // Reflect UV coordinates when out of bounds
                float2 reflectedUV = uv;
                
                // Handle X reflection
                if (reflectedUV.x < 0.0) 
                {
                    reflectedUV.x = -reflectedUV.x;
                }
                else if (reflectedUV.x > 1.0) 
                {
                    reflectedUV.x = 2.0 - reflectedUV.x;
                }
                
                // Handle Y reflection
                if (reflectedUV.y < 0.0) 
                {
                    reflectedUV.y = -reflectedUV.y;
                }
                else if (reflectedUV.y > 1.0) 
                {
                    reflectedUV.y = 2.0 - reflectedUV.y;
                }
                
                return tex2D(tex, reflectedUV);
            }

            half4 sampleTextureWithParallax(sampler2D tex, float2 baseUV, float2 parallaxOffset)
            {
                // Simple sampling without supersampling for better performance
                float2 parallaxUV = baseUV + parallaxOffset;
                return sampleTexture(tex, parallaxUV);
            }

            
            fixed4 frag(v2f IN) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;

                // Map position to size of box
                float2 size = _RectSize.xy;
                float2 pos = (IN.texcoord * 2 - 1) * size;

                // Optimized noise calculations - batch similar operations
                float3 noiseInput1 = float3(pos*5, _Time.z);
                float3 noiseInput2 = float3(pos*10, _Time.w);
                float3 noiseInput3 = float3(pos*30, _Time.w*2);
                float2 posR = mul(GOLD, pos);
                float3 noiseInputR = float3(posR*7, _Time.w);

                // Calculate all noise values
                float noise1 = dot_noise(noiseInput1) * 0.3;
                float noise2 = dot_noise(noiseInput2) * 0.3;
                float noise3 = dot_noise(noiseInput3) * 0.05;
                float noiseR = dot_noise(noiseInputR) * 0.1;
                float noiseB_offset = dot_noise(float3(pos*10, _Time.w + 2)) * 0.3;

                // Combine noise values
                float noise = 0.8 - abs(noise1) - abs(noiseR);
                float noiseG = noise2 + noise3;
                float noiseB = noiseB_offset + noise3;

                // Rounded rect SDF
                float3 sdg = sdgRoundedBox(pos, size, _Radius * 2) - noise*0.05;
                float dist = sdg.x;

                // Cache frequently used smoothstep calculations
                float smooth_neg05_neg3 = smoothstep(-0.05, -0.3, dist);
                float smooth_0_neg5 = smoothstep(0, -0.5, dist);
                float smooth_neg1_neg2 = smoothstep(-0.1, -0.2, dist);
                float smooth_0_neg15 = smoothstep(-0.0, -0.15, dist);
                float smooth_neg02_neg5 = smoothstep(-0.02, -0.5, dist);
                float smooth_neg2_neg6 = smoothstep(-0.2, -0.6, dist);
                float smooth_neg2_neg45 = smoothstep(-0.2, -0.45, dist);

                // Calculate parallax offset based on view direction
                float distortion = smooth_neg05_neg3 + 0.2;
                distortion += (1 - smooth_0_neg5) * noise;

                float2 parallaxOffset = IN.viewDir.xy / (IN.viewDir.z * distortion + _ParallaxDepth) * _ParallaxStrength;

                // Sample texture
                half4 textureSample = sampleTextureWithParallax(_MainTex, IN.texcoord, parallaxOffset);

                half4 color = IN.color * (textureSample + _TextureSampleAdd);

                // Remove pixels outside the rounded rectangle
                float mask = smooth_neg1_neg2;
                color += (1-mask);
                color.a = mask*mask;

                // Cache border mask calculations
                float outerBorderMask = pow(smooth_0_neg15, 3);
                float innerBorderMask = pow(smooth_neg02_neg5, 0.4);

                // Add border
                float borderMask = outerBorderMask - innerBorderMask;
                color += borderMask*0.8;

                // Caustic lines
                borderMask = outerBorderMask - pow(smooth_neg2_neg6, 0.2);
                color += borderMask*pow(noise,4)*2;

                // Colorful border
                borderMask = outerBorderMask - pow(smooth_neg2_neg45, 0.4);
                color += saturate(float4(noise,noiseG,noiseB,0) * borderMask) * 0.4;


                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.a *= IN.color.a;
                color.rgb *= color.a;

                return color;
            }
        ENDCG
        }
    }
}
