Shader "Custom/ProjectorTarget/Texture"
{
    Properties
    {
        [MaterialToggle] _IsProjectedToFront ("IsProjectedToFront", Float) = 1
        _Opacity ("Opacity", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
	    Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 projectorSpacePos : TEXCOORD0;
            };

            // private properties
            float _IsProjectedToFront;
            float _Opacity;
            
            // public properties
            sampler2D _ProjectorTexture;
            float4x4 _ProjectorMatrixVP;
            float4 _ProjectorPos;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                o.projectorSpacePos = mul(mul(_ProjectorMatrixVP, unity_ObjectToWorld), v.vertex);
                o.projectorSpacePos = ComputeScreenPos(o.projectorSpacePos);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                i.projectorSpacePos.xyz /= i.projectorSpacePos.w;
                float2 uv = i.projectorSpacePos.xy;
                float4 projectorTex = tex2D(_ProjectorTexture, uv);
                projectorTex.a = _Opacity;

                fixed3 isOut = step((i.projectorSpacePos - 0.5) * sign(i.projectorSpacePos), 0.5);
                float cutoff = isOut.x * isOut.y * isOut.z;
                float isFrontVal = (_IsProjectedToFront - 0.5) * 2.0;
                cutoff *= step(isFrontVal * dot(lerp(-_ProjectorPos.xyz, _ProjectorPos.xyz - i.worldPos, _ProjectorPos.w), i.worldNormal), 0);
                
                return projectorTex * cutoff;
            }
            ENDCG
        }
    }
}