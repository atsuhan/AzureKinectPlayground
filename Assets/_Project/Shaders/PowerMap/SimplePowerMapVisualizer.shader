Shader "Custom/SimplePowerMapVisualizer"
{
    Properties
    {
        _ColorMap ("ColorMap", 2D) = "white" {}
        _Opacity ("Opacity", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../_lib/GetColorWithBuffer.h"

            // private properties
            sampler2D _ColorMap;
            float _Opacity;

            // public properties
            StructuredBuffer<float> _PowerBuffer;
            int _PowerBufferWidth;
            int _PowerBufferHeight;

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

            float4 frag (v2f i) : SV_Target
            {
                PropsColorWithBuffer props;
                props.colorMapTex = _ColorMap;
                props.uv = i.uv;
                props.buffer = _PowerBuffer;
                props.bufferWidth = _PowerBufferWidth;
                props.bufferHeight = _PowerBufferHeight;
                float4 powerColor = GetColorWithBuffer(props);

                powerColor.a = _Opacity;

                return powerColor;
            }
            ENDCG
        }
    }
}
