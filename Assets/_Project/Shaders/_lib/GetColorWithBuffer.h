struct PropsColorWithBuffer {
    sampler2D colorMapTex;
    float2 uv : TEXCOORD0;

    StructuredBuffer<float> buffer;
    int bufferWidth;
    int bufferHeight;
};

float4 GetColorWithBuffer(PropsColorWithBuffer props) : COLOR0
{
    int indexX = floor(props.uv.x * props.bufferWidth);
    int indexY = floor(props.uv.y * props.bufferHeight);
    int bufferId = indexX + indexY * props.bufferWidth;
    float powerVal = props.buffer[bufferId];

    float2 colorMapPoint = float2(powerVal, 0);
    return tex2D(props.colorMapTex, colorMapPoint);
}