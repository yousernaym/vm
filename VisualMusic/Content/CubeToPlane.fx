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
	MinFilter = ANISOTROPIC;
	MagFilter = ANISOTROPIC;
	MipFilter = ANISOTROPIC;
	MaxAnisotropy = 8;
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
	//return getColor(IN.planeCoords);
	float4 cmSample = float4(0,0,0,0);
	float sampleOffsetStep = 0.5f / ViewportSize; //Somple points are halfway to next pixel
	float cornerWeight = 0.5f;
	float straightWeight = 0.5f;
	float centerWeight = 1;
	cmSample += getColor(IN.planeCoords + float2(-1, -1) * sampleOffsetStep) * cornerWeight;
	cmSample += getColor(IN.planeCoords + float2(-1, 1) * sampleOffsetStep) * cornerWeight;
	cmSample += getColor(IN.planeCoords + float2(1, -1) * sampleOffsetStep) * cornerWeight;
	cmSample += getColor(IN.planeCoords + float2(1, 1) * sampleOffsetStep) * cornerWeight;
	cmSample += getColor(IN.planeCoords + float2(0, 0) * sampleOffsetStep) * centerWeight;
	cmSample += getColor(IN.planeCoords + float2(-1, 0) * sampleOffsetStep) * straightWeight;
	cmSample += getColor(IN.planeCoords + float2(1, 0) * sampleOffsetStep) * straightWeight;
	cmSample += getColor(IN.planeCoords + float2(0, -1) * sampleOffsetStep) * straightWeight;
	cmSample += getColor(IN.planeCoords + float2(0, 1) * sampleOffsetStep) * straightWeight;

	float totalWeights = cornerWeight * 4 + centerWeight + straightWeight * 4;
	cmSample /=  totalWeights * FrameSamples;
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
