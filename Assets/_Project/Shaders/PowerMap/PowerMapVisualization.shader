Shader "Custom/PowerMapVisualization"
{
    Properties
    {
        _ColorMap ("ColorMap", 2D) = "white" {}
        _Opacity ("Opacity", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "/Functions/GetColorFromTexture.h"

            // vert
            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
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

            // frag
            sampler2D _ColorMap;
            float _Opacity;

            int _ResolutionX;
            int _ResolutionY;
            StructuredBuffer<float> _PowerBuffer;

            float4 frag (v2f i) : SV_Target
            {
                int indexX = floor(i.uv.x * _ResolutionX);
                int indexY = floor(i.uv.y * _ResolutionY);
                int bufferId = indexX + indexY *_ResolutionY;

                float4 powerColor = GetColorFromTex(_ColorMap, float2(_PowerBuffer[bufferId], 0));
                powerColor.a = _Opacity;
                return powerColor;
            }
            ENDCG
        }
    }
}
