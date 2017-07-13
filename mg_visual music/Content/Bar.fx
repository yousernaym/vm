#include "notestyle.fx"

struct VSInput
{
    float2 pos : POSITION0;
	float4 rect : POSITION1;
	float4 texCoords : TEXCOORD0;
	float4 color : COLOR0;
};

struct VSOutput
{
    float4 pos : POSITION0;
	float2 texCoords : TEXCOORD0;
	float4 color : COLOR0;
};

VSOutput VS(VSInput IN)
{
	VSOutput OUT;
	OUT.pos = float4(IN.rect.xy + IN.pos * (IN.rect.zw - IN.rect.xy), 0, 1);
	OUT.pos.xyz += PosOffset;
	// Viewport adjustment.
	//OUT.pos.xy /= ViewportSize.xy;
	//OUT.pos.xy *= float2(2, -2);
	//OUT.pos.xy -= float2(1, -1);
	OUT.pos = mul(OUT.pos, WvpMat);
	//OUT.pos.z = 0;
	//OUT.pos.w = 1;
	OUT.texCoords = IN.texCoords.xy + IN.pos * (IN.texCoords.zw - IN.texCoords.xy);
	OUT.color = IN.color;
    return OUT;
}

float4 PS(VSOutput IN) : COLOR0
{
	float4 color = IN.color;
	color.rgb *= tex2D(TextureSampler, IN.texCoords);
    return color;
	//return float4(1, 1, 1, 1);
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
