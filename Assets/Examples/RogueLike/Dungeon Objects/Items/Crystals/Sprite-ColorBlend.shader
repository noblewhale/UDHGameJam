Shader"Unlit/Sprite-ColorBlend" {
    Properties {
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _MinMaxX("MinMax Texture X", Vector) = (0, 1, 0, 0)
        _GradientOffsets("Gradient Offsets", Vector) = (0, 0, 0, 0)
        [HDR]_FirstColor("First Color", Color) = (1, 1, 1, 1)
        [HDR]_SecondColor("Second Color", Color) = (1, 1, 1, 1)
        [HDR]_ThirdColor("Third Color", Color) = (1, 1, 1, 1) 
    }
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Lighting Off

        Pass {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    float2 texcoord2 : TEXCOORD1;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _GradientOffsets;
                float4 _FirstColor;
                float4 _SecondColor;
                float4 _ThirdColor;
                float4 _MinMaxX;

                v2f vert (appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.texcoord2 = v.texcoord;
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord);
                    float f = (i.texcoord2.x - _MinMaxX.x) / (_MinMaxX.y - _MinMaxX.x);
                    if (f < .5)
                    {
                        f -= _GradientOffsets.x;
                        f *= 2;
                        col.rgb *= (1 - f) * _FirstColor + f * _SecondColor;
                    }
                    else
                    {
                        f -= .5;
                        f += _GradientOffsets.y;
                        f *= 2;
                        col.rgb *= (1 - f) * _SecondColor + f * _ThirdColor;
                    }
                    return col;
                }
            ENDCG
        }
    }
}
