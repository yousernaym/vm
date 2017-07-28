float4x4 WvpMat;
float2 ViewportSize;
float2 TexSize;
texture Texture;
float2 ProjScale;

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
float3 PosOffset;

#define ModEntriesCount 5

float2 Origin[ModEntriesCount];
int XSource[ModEntriesCount];
int YSource[ModEntriesCount];
int CombineXY[ModEntriesCount];
bool ColorDestEnable[ModEntriesCount];
bool AngleDestEnable[ModEntriesCount];
float4 ColorDest[ModEntriesCount];
int AngleDest[ModEntriesCount];
float Start[ModEntriesCount];
float Stop[ModEntriesCount];
float FadeIn[ModEntriesCount];
float FadeOut[ModEntriesCount];
float Power[ModEntriesCount];
float Scale[ModEntriesCount];

//bool bla[4][3];
int ActiveModEntries;
//##define ModSource_TopLeft 0
//##define Center 1
//##define BottomRight 2
#define CombineXY_Add 0
#define CombineXY_Mul 1
#define CombineXY_Max 2
#define CombineXY_Min 3

float getInterpolant(float2 pos, int modIndex)
{
	float2 transformedPos = pos - Origin[modIndex];
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
	if (CombineXY[modIndex] == CombineXY_Add)
		interpolant = distFromOrigin.x + distFromOrigin.y;
	else if (CombineXY[modIndex] == CombineXY_Mul)
		interpolant = sqrt(distFromOrigin.x * distFromOrigin.y);
	else if (CombineXY[modIndex] == CombineXY_Max)
		interpolant = max(distFromOrigin.x, distFromOrigin.y);
	else if (CombineXY[modIndex] == CombineXY_Min)
		interpolant = min(distFromOrigin.x, distFromOrigin.y);
	//interpolant = distFromOrigin.x;
	
	if (interpolant < Start[modIndex] || interpolant > Stop[modIndex])
		return 0;
	interpolant -= Start[modIndex];
	interpolant /= Stop[modIndex] - Start[modIndex];
	
	if (interpolant > FadeIn[modIndex] && 1 - interpolant > FadeOut[modIndex])
		return 1;
	
	float fadeInInterpolant;
	if (FadeIn[modIndex] > 0 && interpolant < FadeIn[modIndex])
		fadeInInterpolant =  interpolant / FadeIn[modIndex];
	else
		fadeInInterpolant = 1;

	float fadeOutInterpolant;
	if (FadeOut[modIndex] > 0 && 1 - interpolant < FadeOut[modIndex])
		fadeOutInterpolant = (1 - interpolant) / FadeOut[modIndex];
	else
		fadeOutInterpolant = 1;
	interpolant = fadeOutInterpolant * fadeInInterpolant;
	
	interpolant = pow(interpolant, Power[modIndex]) * Scale[modIndex];
	return interpolant;
}
float3 modulateColor(float2 coord, float3 colorSource)
{
	float3 result = colorSource;
	for (int i = 0; i < ActiveModEntries; i++)
	{
		if (!ColorDestEnable[i])
			continue;
		float interpolant = getInterpolant(coord, i);
		//return float3(interpolant.xxx);
		result = lerp(result, ColorDest[i], interpolant);
	}
	return result;
}

