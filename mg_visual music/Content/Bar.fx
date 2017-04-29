// Input parameters

//Common parameters-----------
float4x4 wvp;
float2   ViewportSize;
float2   TexSize;
texture Texture;

sampler  TextureSampler = sampler_state
{
	texture = <Texture>;
};
float4 Color;
float BlurredEdge;
float3 LightDir = normalize(float3(1, 1, 1));
float AmbientLum = 0.25f;
float SpecAmount;
float SpecPower;
float SpecFov;
float3 SpecCamPos;
//------------------------------

struct VSInput
{
    float4 pos : POSITION0;
	float4 texCoords : TEXCOORD0;
};

struct VSOutput
{
    float4 pos : POSITION0;
	float4 texCoords : TEXCOORD0;
};

VSOutput VS(VSInput input)
{
	VSOutput output;
	output.pos = input.pos;
	output.texCoords = input.texCoords;
    //float4 worldPosition = mul(input.Position, World);
    //float4 viewPosition = mul(worldPosition, View);
    //output.Position = mul(viewPosition, Projection);
	//output.pos = mul(input.pos, wvp);

	return output;
}

float4 PS(VSOutput input) : COLOR0
{
	float4 color = Color;
	color.rgb *= tex2D(TextureSampler, input.texCoords);
    return color;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        //VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_5_0 PS();
    }
}
