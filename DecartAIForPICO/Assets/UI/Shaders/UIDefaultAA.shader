Shader "Unlit/UIDefaultAA"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

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
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
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
                float2 uv  : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            float2 _RectSize;
            float4 _CornerRadius;
            float4 _Margin; // (Top, Left, Bottom, Right)
            float4 _CropMargin; // (Top, Left, Bottom, Right)

            // Applies margin to uv coordinates based on the given size
            float2 applyMargin(float2 uv, float2 size, float4 m)
            {
                const float2 innerSize = float2( // Size without margin
                    size.x - m.y - m.w,
                    size.y - m.x - m.z);

                const float2 scalingRatio = size / innerSize;
                const float2 marginOffset = float2(m.y / size.x, m.x / size.y);
                
                return scalingRatio * (uv  - marginOffset);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                float4 vPosition = UnityObjectToClipPos(v.vertex);

                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float2 uv = applyMargin(v.texcoord.xy, _RectSize, _Margin);

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                OUT.texcoord = TRANSFORM_TEX(uv, _MainTex);
                OUT.uv = abs(v.texcoord.xy); // raw UV
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            // Signed distance field of a rounded rectangle
            // https://iquilezles.org/articles/distfunctions2d/
            // top-left | top-right | bottom-right | bottom-left
            float sdRoundedBox(float2 position, float2 size, half4 radius)
            {
                radius = radius.yzxw;
                radius.xy = (position.x > 0.0) ? radius.xy : radius.zw;
                radius.x  = (position.y > 0.0) ? radius.x  : radius.y;
                float2 dist = abs(position) - size + radius.x;
                return min(max(dist.x, dist.y), 0.0) + length(max(dist, 0.0)) - radius.x;
            }

            half4 sampleTextureClamp01(sampler2D tex, float2 uv)
            {
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return float4(0.0, 0.0, 0.0, 0.0);
                }
                return tex2D(tex, uv);
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;


                half4 textureSample = 0.0;
                // Rotated Grid MSAA
                // per pixel partial derivatives
                float2 dx = ddx(IN.texcoord);
                float2 dy = ddy(IN.texcoord);

                // rotated grid uv offsets
                float2 uvOffsets = float2(0.125, 0.375);
                float2 offsetUV = float2(0.0, 0.0);

                // Supersample
                offsetUV.xy = IN.texcoord + uvOffsets.x * dx + uvOffsets.y * dy;
                textureSample += sampleTextureClamp01(_MainTex, offsetUV);
                float _UseMSAA = 0.0;
                if (_UseMSAA)
                {
                    offsetUV.xy = IN.texcoord - uvOffsets.x * dx - uvOffsets.y * dy;
                    textureSample += sampleTextureClamp01(_MainTex, offsetUV);
                    offsetUV.xy = IN.texcoord + uvOffsets.y * dx - uvOffsets.x * dy;
                    textureSample += sampleTextureClamp01(_MainTex, offsetUV);
                    offsetUV.xy = IN.texcoord - uvOffsets.y * dx + uvOffsets.x * dy;
                    textureSample += sampleTextureClamp01(_MainTex, offsetUV);
                    textureSample *= 0.25;
                }
                
                half4 color = IN.color * (textureSample + _TextureSampleAdd);

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;
                
                if (_UseMSAA)
                {
                    // Antialias & round edges
				    float2 pos = (IN.uv * 2 - 1) * _RectSize; // Normalized position of the fragment in the rect
                    float4 margin = _Margin + _CropMargin;
                    float2 marginOffset = float2(margin.y - margin.w, margin.x - margin.z);
                    pos -= marginOffset;
                    float2 size = float2(
                        _RectSize.x - 1 * (margin.y + margin.w),
                        _RectSize.y - 1 * (margin.x + margin.z));
                    
				    float dist = sdRoundedBox(pos, size, _CornerRadius);
				    float pixelWidth = fwidth(dist) * 1.3; // Get size of a pixel to determine falloff
				    float antialiasingMask = smoothstep(0.0, -pixelWidth, dist);
                    color.a *= antialiasingMask;
                }
                return fixed4(1,0,0,1);
            }
        ENDCG
        }
    }
}
