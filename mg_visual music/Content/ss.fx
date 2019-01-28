texture FrameTex;
sampler2D Sampler = sampler_state
{
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    texture = <FrameTex>;
};

struct VsInput
{
	float2 pos : POSITION0;
};

struct VsOutput
{
	float4 pos : POSITION0;
    float2 tc : TEXCOORD0;
};

VsOutput VS(in VsInput input)
{
    VsOutput output;
    output.pos = float4(input.pos, 0, 1);
    output.tc = input.pos * float2(0.5f, -0.5f) + 0.5f;
	return output;
}

float4 PS(VsOutput input) : COLOR
{
    //return float4(1, 0, 1, 1);
	return tex2D(Sampler, input.tc.xy);
}

technique BasicDrawing
{
	pass P0
	{
		VertexShader = compile vs_5_0 VS();
		PixelShader = compile ps_5_0 PS();
	}
};