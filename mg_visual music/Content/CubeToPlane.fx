const float PI = 3.1415926535f;
float2 Viewport;

// Input parameters
texture CubeMap;
samplerCUBE  CubeMapSampler = sampler_state
{
	texture = <CubeMap>;
};

struct VSInput
{
	float2 pos : POSITION0;
};

struct VSOutput
{
	float4 pos : POSITION0;
	float2 planeCoords : TEXCOORD0;
};

VSOutput VS(VSInput IN)
{
	VSOutput OUT;
	OUT.pos = float4(IN.pos, 0, 1);
	OUT.planeCoords = IN.pos;
	return OUT;
}


float4 getColor(float2 planeCoords)
{
	float theta = -planeCoords.x * PI;
	float phi = planeCoords.y * PI / 2.0f;
	//float phi = (float)Math.Asin(y);
	float3 cmCoords;
	cmCoords.x = cos(phi) * cos(theta);
	cmCoords.y = sin(phi);
	cmCoords.z = cos(phi) * sin(theta);
	return texCUBE(CubeMapSampler, normalize(cmCoords));
}

float4 PS(VSOutput IN) : COLOR0
{
	float4 cmSample = float4(0,0,0,0);
	float2 sampleOffsetStep = 0.5f / Viewport;
	cmSample = getColor(IN.planeCoords);
	for (int j = 0; j < 2; j++)
	{
		for (int i = 0; i < 2; i++)
		{
			float2 currentOffset = -sampleOffsetStep + sampleOffsetStep * 2 * float2(i, j);
			cmSample += getColor(IN.planeCoords + currentOffset);
		}
	}	
	return cmSample / 5;
}


technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VS();
		PixelShader = compile ps_5_0 PS();
	}
}
