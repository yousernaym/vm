/*float3x3 SampleWeights = { 0.5 , 0.75, 0.5 ,
						   0.75, 1   , 0.75,
						   0.5 , 0.75, 0.5 };*/
float3x3 SampleWeights = { 0.75, 0.5, 0.75,
						   0.5 ,  1, 0.5 ,
						   0.75, 0.5, 0.75 };
float PI = 3.1415926535f;
float2 ViewportSize;
float2 PrevFrameScaleOffset;
float FrameSamples = 1;
bool IsFirstFrame;
float FovLimit; //Cos of FOV angle. Clip if dot(lookat_dir, cubemap_lookup_vector) < FovLimit. FovLimit = -1 means no clipping.

// Input parameters
texture CubeMap;
samplerCUBE  CubeMapSampler = sampler_state
{
	/*MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	MipFilter = ANISOTROPIC;*/
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	//MaxAnisotropy = 8;
	texture = <CubeMap>;
};

texture PrevFrame;
sampler2D PrevFrameSampler = sampler_state
{
	texture = <PrevFrame>;
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
	OUT.pos = float4(IN.pos*float2(1,-1), 0, 1); //Flip y-coord when writing mp4 videos
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
    if (dot(normalize(cmCoords), float3(1, 0, 0)) < FovLimit)
		return float4(0, 0, 0, 0);
	return texCUBE(CubeMapSampler, normalize(cmCoords));
}

float4 PS(VSOutput IN) : COLOR0
{
	float4 cmSample = float4(0,0,0,0);
	float2 sampleOffsetStep = 0.5f / ViewportSize;
	//return cmSample = getColor(IN.planeCoords);
	for (int j = -1; j <= 1; j++)
	{
		for (int i = -1; i <= 1; i++)
		{
			float2 currentOffset = sampleOffsetStep * float2(i, j);
			cmSample += getColor(IN.planeCoords + currentOffset) * SampleWeights[i+1][j+1];
		}
	}	
	cmSample /= 6 * FrameSamples;
	if (!IsFirstFrame)
		cmSample += tex2D(PrevFrameSampler, IN.planeCoords * float2(0.5f, 0.5f * PrevFrameScaleOffset.x) + float2(0.5f, 0.5 + PrevFrameScaleOffset.y));
	return cmSample;
}


technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VS();
		PixelShader = compile ps_5_0 PS();
	}
}
