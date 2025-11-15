Shader "UI/UIRect"
{
    Properties
    {
        // Default UI shader properties
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Mask support
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
            #include "Utils.cginc"
            #include "BlurredRect.cginc"
            
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #pragma multi_compile_local __ _USE_BEVELS

 
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                half4 color : COLOR;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
				float4 uv3 : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR0;
                half4 fillColor : COLOR1;
                half4 borderColor : COLOR2;
                float2 uv : TEXCOORD0;
                float2 size : TEXCOORD1;
                float4 radii : TEXCOORD2;
                float4 uv2 : TEXCOORD3;
                float4 uv3 : TEXCOORD5;
                float4 worldPosition : TEXCOORD4;
                
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            sampler2D _MainTex;
            half4 _Color;
            half4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            #define BOX_RENDER_MODE_FILL 0
            #define BOX_RENDER_MODE_SHADOW 1
            #define BOX_RENDER_MODE_BEVEL 2

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = mul(unity_ObjectToWorld, v.vertex);
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                
                OUT.uv = TRANSFORM_TEX(v.uv0, _MainTex);
                OUT.size = v.uv1.xy;

                // Unpack data here to prevent per-fragment calculations
                float2 top = unpack2floats(v.uv1.z) * v.uv1.x; // Decode radii and un-normalize (multiply by width)
				float2 bottom = unpack2floats(v.uv1.w) * v.uv1.x;
                OUT.radii = float4(top, bottom);
                OUT.color = v.color * _Color;
                OUT.uv2 = v.uv2;
                OUT.uv3 = v.uv3;
                OUT.fillColor = unpackColor(v.uv2.x);
                OUT.borderColor = unpackColor(v.uv2.y);

                return OUT;
            }
 
            half4 frag(v2f IN) : SV_Target
            {
                half4 texColor = pow(tex2D(_MainTex, IN.uv) + _TextureSampleAdd, 1/2.2);
                half4 color = texColor * IN.color * IN.fillColor;
                
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                int boxRenderMode = IN.uv3.x;
                float effectWidth = IN.uv2.z;

                float2 pos = (IN.uv * 2 - 1) * IN.size; // Map position to size of box
                float3 sdg = sdgRoundedBox(pos, IN.size, IN.radii * 2);
                float dist = sdg.x;
                float pixelWidth = fwidth(dist) * 1.1;

                float outerDist = 0;

                // Add border
                if (effectWidth > 0 && boxRenderMode == BOX_RENDER_MODE_FILL)
                {
                    float borderOffset = IN.uv2.w;

                    // Use at least pixelWidth to prevent aliasing
                    float width = max(effectWidth, pixelWidth);
                    float thinBorderRatio = effectWidth / width;
                    
                    // Calc distances
                    outerDist = lerp(0, width * borderOffset, thinBorderRatio);
                    float borderInnerDist = lerp(-width, -width * (1 - borderOffset), thinBorderRatio);

                    // If the border is thinner than a pixel, fade border color according to pixel coverage
                    half borderAlpha =  IN.color.a * IN.borderColor.a * thinBorderRatio * (1 - smoothstep(borderInnerDist, borderInnerDist - pixelWidth, dist));
                    half4 borderColor =  float4(IN.borderColor.xyz, borderAlpha);
                    
                    color = overlayColors(color, borderColor);
                }

                #ifdef _USE_BEVELS
                float bevelWidth = IN.uv3.y * 2;
                if (bevelWidth > 0 && boxRenderMode != BOX_RENDER_MODE_SHADOW)
                {
                    float3 neutralNormal = float3(0, 0, 1); // The base surface normal of the quad
                    float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPosition.xyz);
                    viewDir = UnityWorldToObjectDir(viewDir);
                    float bevelStrength = IN.uv3.z;

                    // parallaxMapping
                    float2 newUV = parallaxMapping(IN.uv, viewDir, 0.0005 * bevelWidth);
                    // float2 newUV = IN.uv;
                    float2 newPos = (newUV * 2 - 1) * IN.size;
                    sdg = sdgRoundedBox(newPos, IN.size, IN.radii * 2);
                    float bevelDist = smoothstep(outerDist - bevelWidth, outerDist, sdg.x);
                    // end parallaxMapping
                    
                    float2 g = sdg.yz * bevelStrength * bevelDist;  // Fade bevel further from edge
                    float3 normal = normalize(float3(g.xy, 1)); // n = (1, 0, ∂x) ⨯ (0, 1, ∂y) = (-∂x, -∂y, 1)

                    // Calculate Blinn reflectance
                    float3 lightDir = normalize(float3(0, 1, 1)); // Directional light
                    half3 reflection = reflect(lightDir, normal);
                
                    float shininess = 10;
                    float specular = pow(max(dot(viewDir, reflection), 0), shininess);

                    float neutralShading = dot(neutralNormal, lightDir);
                    float shading = dot(normal, lightDir);
                    shading = shading - neutralShading;
                    shading = shading < 0 ? shading * 0.6 : shading;
                    shading = abs(shading)*0.1 + 0.5;

                    shading += specular * 0.2 * bevelDist;
                    color.rgb = saturate((shading * 2 - 1) * bevelStrength + color.rgb);

                    // color = bevelDist;
                }
                #endif
                

                if (boxRenderMode == BOX_RENDER_MODE_SHADOW)
                {
                    float shadowSpread = IN.uv3.z;
                    
                    // Use at least pixelWidth blur to prevent aliasing
                    float blur = max(effectWidth, pixelWidth / 3);
                    float antialiasingOffset = shadowSpread - pixelWidth;
                    float2 size = IN.size + antialiasingOffset.xx;
                    float4 radius = IN.radii * 2 + antialiasingOffset.xxxx;
                    
                    color.a *= roundedBoxShadow(pos, size, blur, radius);
                    return color;
                }

                // Remove pixels outside the outer border
                color.a *= smoothstep(outerDist, outerDist - pixelWidth, dist);

                color.rgb = pow(color.rgb, 2.2);

                return color;// + color.a * 0.0 * random2(IN.uv);
            }
        ENDCG
        }
    }
}