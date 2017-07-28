#include "notestyle.fx"

struct VSInput
{
    float2 normPos : POSITION0;
	float4 rect : POSITION1;
	float4 texCoords : TEXCOORD0;
	float4 color : COLOR0;
};

struct VSOutput
{
    float4 pos : POSITION0;
	float2 normPos : POSITION1;
	float4 worldPos : POSITION2;
	float2 texCoords : TEXCOORD0;
	float4 color : COLOR0;
};

VSOutput VS(VSInput IN)
{
	VSOutput OUT;
	OUT.worldPos = float4(IN.rect.xy + IN.normPos * IN.rect.zw, 0, 1); //top-left + 0|1 * size
	OUT.worldPos.xyz += PosOffset;
	// Viewport adjustment.
	//OUT.pos.xy /= ViewportSize.xy;
	//OUT.pos.xy *= float2(2, -2);
	//OUT.pos.xy -= float2(1, -1);
	OUT.pos = mul(OUT.worldPos, VpMat);
	//OUT.pos.z = 0;
	//OUT.pos.w = 1;
	OUT.texCoords = IN.texCoords.xy + IN.normPos * IN.texCoords.zw;
	OUT.color = IN.color;
	OUT.normPos = IN.normPos;
    return OUT;
}

float4 PS(VSOutput IN) : COLOR0
{
	float4 color = IN.color;
	color.rgb *= tex2D(TextureSampler, IN.texCoords);
	color = modulateColor(IN.normPos, color, IN.worldPos);
	
	return color;
	//return float4(, 0, 0, color.r*10);
	//return float4(ColorDest[0])+float4(0,0,0,color.x);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}
