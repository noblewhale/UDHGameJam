// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Sprite Feather Masked"
{
  Properties
  {
      [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
      [PerRendererData] _Mask("Mask", 2D) = "white" {}
      _Color("Tint", Color) = (1,1,1,1)
      [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
      [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
      [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
      [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
      [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
  }

    SubShader
      {
          Tags
          {
              "Queue" = "Transparent"
              "IgnoreProjector" = "True"
              "RenderType" = "Transparent"
              "PreviewType" = "Plane"
              "CanUseSpriteAtlas" = "False"
          }

          Cull Off
          Lighting Off
          ZWrite Off
          Blend SrcAlpha OneMinusSrcAlpha

          Pass
          {
          CGPROGRAM
              #pragma vertex SpriteVert
              #pragma fragment SpriteFeatherMaskedFrag
              #pragma target 2.0
              #pragma multi_compile_instancing
              #pragma multi_compile_local _ PIXELSNAP_ON
              #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
              #include "UnitySprites.cginc"

              sampler2D _Mask;

              v2f SpriteVertShifted(appdata_t IN)
              {
                  v2f OUT;

                  UNITY_SETUP_INSTANCE_ID(IN);
                  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                  OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                  OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                  OUT.texcoord = IN.texcoord;
                  OUT.color = IN.color * _Color * _RendererColor;

                  #ifdef PIXELSNAP_ON
                  OUT.vertex = UnityPixelSnap(OUT.vertex);
                  #endif

                  return OUT;
              }

              float4 SpriteFeatherMaskedFrag(v2f IN) : COLOR
              {
                float4 color = tex2D(_MainTex, IN.texcoord);
                color.a = (1-tex2D(_Mask, IN.texcoord).a);
                return color;
              }
          ENDCG
          }
      }
}
