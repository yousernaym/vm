float4x4 VpMat;
float2 ViewportSize;
texture Texture;
float2 ProjScale;
float SongPos;
float2 TexScrollOffset;
float VertWidthScale;
float TexWidthScale;


sampler  TextureSampler = sampler_state
{
	texture = <Texture>;
};
float4 Color;
float BlurredEdge;
float3 LightDir = normalize(float3(1, 1, 1));
float AmbientAmount;
float DiffuseAmount;
float SpecAmount;
float SpecPower;
float SpecFov;
float3 CamPos;
float3 PosOffset;

#define ModEntriesCount 5

float2 Origin[ModEntriesCount];
bool XOriginEnable[ModEntriesCount];
bool YOriginEnable[ModEntriesCount];
int CombineXY[ModEntriesCount];
bool ColorDestEnable[ModEntriesCount];
bool AlphaDestEnable[ModEntriesCount];
bool AngleDestEnable[ModEntriesCount];
float4 ColorDest[ModEntriesCount];
float AngleDest[ModEntriesCount];
float Start[ModEntriesCount];
float Stop[ModEntriesCount];
float FadeIn[ModEntriesCount];
float FadeOut[ModEntriesCount];
float Power[ModEntriesCount];
bool DiscardAfterStop[ModEntriesCount];
bool Invert[ModEntriesCount];

//bool bla[4][3];
int ActiveModEntries;
//##define ModSource_TopLeft 0
//##define Center 1
//##define BottomRight 2
#define CombineXY_Add 0
#define CombineXY_Length 1
#define CombineXY_Max 2
#define CombineXY_Min 3

float4 blurEdges(float4 color, float normPosY)
{
	float normDistFromCenter = abs(normPosY - 0.5f) * 2; // 1 at edge, 0 at center

	float BlurredEdgePixels = 2;
	float ddxy = length(float2(ddx(normDistFromCenter), ddy(normDistFromCenter)));

	//If line radius is less than number of pixels to blur + 2, than decrease number of pixels to blur accordingly
	if (ddxy > 1 / (BlurredEdgePixels + 2))
		BlurredEdgePixels = 1 / ddxy - 2;
	if (BlurredEdgePixels < 1.5f)
		return color;

	float blurredEdgePercent = BlurredEdgePixels * ddxy;

	float shadeAmount = max(normDistFromCenter - (1 - blurredEdgePercent), 0) / blurredEdgePercent; // 0 from center to blur start, 1 at edge
																									//if (shadeAmount > 0)
																									//return float4(0, 1, 0, 1);
	float lum = saturate(1 - shadeAmount);
	return color * lum;
}

float3 calcLighting(float3 color, float3 normal, float3 worldPos)
{
	//float lum = clamp(dot(LightDir, normal), AmbientLum, 1);
	color *= saturate(dot(LightDir, normal)) * DiffuseAmount + AmbientAmount;
	float3 lightReflection = -reflect(LightDir, normal);
	float3 viewVec = normalize(CamPos - worldPos);
	if (any(color))
		color += pow(saturate(dot(lightReflection, viewVec)), SpecPower) * SpecAmount;
	return color;
}

float getInterpolant(float2 normPos, int modIndex, out float3 destNormalDir, out bool discardFade)
{
	discardFade = false;
	float2 transformedPos = normPos - Origin[modIndex];
	if (transformedPos.x < 0)
		transformedPos.x /= Origin[modIndex].x;
	else
		transformedPos.x /= 1 - Origin[modIndex].x;
	if (transformedPos.y < 0)
		transformedPos.y /= Origin[modIndex].y;
	else
		transformedPos.y /= 1 - Origin[modIndex].y;

	float2 distFromOrigin = abs(transformedPos);
	float interpolant = 0;
	destNormalDir = float3(0, 0, 0);

	if (XOriginEnable[modIndex] && YOriginEnable[modIndex])
	{
		if (CombineXY[modIndex] == CombineXY_Max)
		{
			if (distFromOrigin.x > distFromOrigin.y)
				destNormalDir.x = transformedPos.x;
			else
				destNormalDir.y = transformedPos.y;
			interpolant = max(distFromOrigin.x, distFromOrigin.y);
		}
		else if (CombineXY[modIndex] == CombineXY_Min)
		{
			if (distFromOrigin.x < distFromOrigin.y)
				destNormalDir.x = transformedPos.x;
			else
				destNormalDir.y = transformedPos.y;
			interpolant = min(distFromOrigin.x, distFromOrigin.y);
		}
		else //Both x and y is used
		{
			destNormalDir = float3(transformedPos, 0);
			if (CombineXY[modIndex] == CombineXY_Add)
				interpolant = distFromOrigin.x + distFromOrigin.y;
			else if (CombineXY[modIndex] == CombineXY_Length)
				interpolant = pow(distFromOrigin.x, 2) + pow(distFromOrigin.y, 2);
		}
	}
	else if (XOriginEnable[modIndex])
	{
		destNormalDir.x = transformedPos.x;
		interpolant = distFromOrigin.x;
	}
	else if (YOriginEnable[modIndex])
	{
		destNormalDir.y = transformedPos.y;
		interpolant = distFromOrigin.y;
	}
	else
		return 0;

	destNormalDir = normalize(destNormalDir);
		
	//Outside Start - Stop?
	bool discardBeforeStart = Invert[modIndex] && DiscardAfterStop[modIndex];
	//bool discardBeforeStart = DiscardAfterStop[modIndex];
	bool discardAfterStop = !Invert[modIndex] && DiscardAfterStop[modIndex];
	if (interpolant < Start[modIndex])
		return discardBeforeStart ? -1 : 0;
	if (interpolant > Stop[modIndex])
		return discardAfterStop ? -1 : 0;
			
	interpolant -= Start[modIndex];
	interpolant /= Stop[modIndex] - Start[modIndex];
	
	if (interpolant > FadeIn[modIndex] && 1 - interpolant > FadeOut[modIndex])
		return 1;
	
	float fadeInInterpolant;
	if (FadeIn[modIndex] > 0 && interpolant < FadeIn[modIndex])
	{
		fadeInInterpolant = interpolant / FadeIn[modIndex];
		if (discardBeforeStart)
			discardFade = true;
	}
	else
		fadeInInterpolant = 1;

	float fadeOutInterpolant;
	if (FadeOut[modIndex] > 0 && 1 - interpolant < FadeOut[modIndex])
	{
		fadeOutInterpolant = (1 - interpolant) / FadeOut[modIndex];
		if (discardAfterStop)
			discardFade = true;
	}
	else
		fadeOutInterpolant = 1;
	interpolant = fadeOutInterpolant * fadeInInterpolant;
	
	interpolant = pow(saturate(interpolant), Power[modIndex]);
	return interpolant;
}
float4 modulate(float2 normPos, float4 sourceColor, float3 sourceNormal, float3 worldPos)
{
	float4 result = sourceColor;
	float3 destNormal = float3(0,0,0);
	
	float2 gradLength = ddx(normPos);
	for (int i = 0; i < ActiveModEntries; i++)
	{
		if (ColorDestEnable[i] || AlphaDestEnable[i] || AngleDestEnable[i])
		{
			float3 destNormalDir;
			bool discardFade;
			float interpolant = getInterpolant(normPos, i, destNormalDir, discardFade);
			if (interpolant < 0) //Discard after stop
			{
				result = 0;
				//result = blurEdges(result, -interpolant);
				continue;
			}
			if (Invert[i])
				interpolant = 1 - interpolant;
			if (ColorDestEnable[i])
				result.rgb = lerp(result.rgb, ColorDest[i].rgb, interpolant);
			if (AlphaDestEnable[i])
				result.a = lerp(result.a, ColorDest[i].a, interpolant);
			if (AngleDestEnable[i])
			{
				float angle = AngleDest[i] * interpolant; //lerp from 0 to AngleDest
				float3 perpSourceDest = cross(sourceNormal, destNormalDir);
				float3 perpSource = normalize(cross(perpSourceDest, sourceNormal));
				destNormal += cos(angle) * sourceNormal + sin(angle) * perpSource;
				//float xyLength = sin(angle);
				//destNormal = float3(destNormalDir.x * xyLength, destNormalDir.y * xyLength, sourceNormal.z * cos(angle));
			}
			if (discardFade)
				result *= interpolant;
		}
	}
	if (!any(destNormal))
		destNormal = sourceNormal;
	result.rgb = calcLighting(result.rgb, destNormal, worldPos);
	return result;
}

