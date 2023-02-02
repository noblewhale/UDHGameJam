Shader "Custom/Outline"
{
  SubShader
  { 
    Cull Off
    ZWrite On
    ZTest Always
    Pass
    {
      CGPROGRAM
      #pragma vertex VertMe
      #pragma fragment FragMe
      #include "UnityCG.cginc"

      sampler2D _CameraDepthTexture;
      float _Thickness;
      float4 _Color;

      // For grabbing adjacent pixels for outline stuff
      float2 GetAdjacent(float2 pos, int xDir, int yDir)
      {
        // Calculate adjacent location
        float xDif = _Thickness * xDir;
        float yDif = _Thickness * yDir;
        float2 adjacent = float2(pos.x + xDif, pos.y + yDif);

        return adjacent;
      }

      bool IsOutline(float2 uv)
      {
        // Get some adjacent locations
        float2 adjacentUV1 = GetAdjacent(uv, 1, 1);
        float2 adjacentUV2 = GetAdjacent(uv, -1, -1);
        float2 adjacentUV3 = GetAdjacent(uv, -1, 1);
        float2 adjacentUV4 = GetAdjacent(uv, 1, -1);

        // Sample the depth at the current pixel location as well as the adjacent locations
        float depthMe = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
        float depth0 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, adjacentUV1);
        float depth1 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, adjacentUV2);
        float depth2 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, adjacentUV3);
        float depth3 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, adjacentUV4);

        // If the difference between the depths of opposite pixels is large then this is an outline pixel (maybe)
        bool isOutline = abs(depth0 - depth1) > .8f || abs(depth2 - depth3) > .8f;
        // If current pixel depth is the max among neighbors then don't treat it as outline.
        // This keeps the outline strictly outside of the thing being outlined
        // Otherwise the outline would be centered on the edge and half of it would overlap the thing being outlined.
        isOutline = isOutline && (depthMe != max(max(max(depth0, depth1), depth2), depth3));

        return isOutline;
      }

      struct v2f
      {
        float2 texcoord : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      // Just your standard vertex shader
      v2f VertMe(appdata_base v)
      {
        v2f o;
        o.vertex = v.vertex;
        o.texcoord = (v.vertex.xy + 1.0) * 0.5;
        #if UNITY_UV_STARTS_AT_TOP
          o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
        #endif
        return o;
      }

      fixed4 FragMe(v2f i) : SV_Target
      {
        float depthMe = tex2D(_CameraDepthTexture, i.texcoord).r;
        bool isOutline = IsOutline(i.texcoord);

        return _Color * isOutline;
      }
      ENDCG
    }
  }
}