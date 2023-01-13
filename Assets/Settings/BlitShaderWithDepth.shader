Shader "Hidden/Universal Render Pipeline/Blit Shader With Depth"
{
  SubShader
  {
      Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}
      LOD 100

      Pass
      {
          Name "BlitShaderWithDepth0"
          ZTest Always
          ZWrite Off
          Cull Off
          Lighting Off
          Blend Off
          ColorMask 0

          HLSLPROGRAM
          #pragma vertex FullscreenVert
          #pragma fragment Fragment
          #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION
          #pragma multi_compile _ _USE_DRAW_PROCEDURAL
          #pragma multi_compile_fragment _ DEBUG_DISPLAY

          #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
          #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingFullscreen.hlsl"
          #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
          #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl" // required by the below file (I believe)

          TEXTURE2D_X(_SourceTex);
          SAMPLER(sampler_SourceTex);
          TEXTURE2D_X(_CameraDepthTexture);
          SAMPLER(sampler_CameraDepthTexture);
          
          struct PixelData
          {
            float4 color : SV_Target;
            float  depth : SV_Depth;
          };

          float4 Fragment(Varyings input) : SV_TARGET
          {
              UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
              float2 uv = input.uv;

              half4 col = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_SourceTex, uv);
              half depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv);

              #ifdef _LINEAR_TO_SRGB_CONVERSION
              col = LinearToSRGB(col);
              #endif

              PixelData pd;

              #if defined(DEBUG_DISPLAY)
              half4 debugColor = 0;

              if (CanDebugOverrideOutputColor(col, uv, debugColor))
              {
                 /* pd.color = debugColor;
                  pd.depth = 1;
                  return pd;*/
                return debugColor;
              }
              #endif
              return col;
              /*pd.color = col;
              pd.depth = depth;
              return pd;*/
          }
          ENDHLSL
      }
  }
}
