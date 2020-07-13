float4 GetColorFromTex(sampler2D tex, float2 input) : COLOR0
{
    float4 output;
    input.x = input.x == 0.0 ? 0.0001 : input.x;
    input.y = input.y == 0.0 ? 0.0001 : input.y;
    output = tex2D(tex, input);
    return output;
}