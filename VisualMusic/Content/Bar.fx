#include "notestyle.fx"

struct VSInput
{
    float2 normPos : POSITION0;
	float4 rect : POSITION1;
	float4 texCoords : TEXCOORD0;
};

struct VSOutput
{
    float4 pos : POSITION0;
	float2 normPos : POSITION1;
	float4 worldPos : POSITION2;
	float2 worldSize : POSITION3;
	float2 texCoords : TEXCOORD0;
	float4 color : COLOR0;
};

VSOutput VS(VSInput IN)
{
	VSOutput OUT;
	OUT.worldPos = float4(IN.rect.xy + IN.normPos * IN.rect.zw, 0, 1); //top-left + 0|1 * size
	if (IN.normPos.x == 1)
		OUT.worldPos.x += 0.001f / VertWidthScale;
	OUT.pos = wvpTransform(OUT.worldPos, VertWidthScale);
	TexScrollOffset.x /= VertWidthScale;
	OUT.texCoords = IN.texCoords.xy + IN.normPos * IN.texCoords.zw - TexScrollOffset;
	OUT.texCoords.x *= VertWidthScale;
	float4 scaledRect = IN.rect * VertWidthScale;
    if (scaledRect.x < SongPos && scaledRect.x + scaledRect.z > SongPos)
		OUT.color = HlColor;
	else
		OUT.color = Color;
	OUT.normPos = IN.normPos;// *2 - 1;
    OUT.worldSize.x = scaledRect.z;
    return OUT;
}

float4 PS(VSOutput IN) : COLOR0
{
	float4 color = getPixelColor(IN.color, IN.texCoords);
    color = modulate(IN.normPos, IN.worldSize, color, float3(0, 0, 1), IN.worldPos.xyz);
	
	return color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}
