#include "notestyle.fx"

// Input parameters

float Radius;
int Style;
float3 WorldPos;
float HlSize;
float InnerHlSize;
bool Border;

//Helper functions------------------
float4 shadeHlObject(float sgnDistFromEdge, float distFromCenter);

float4 blurEdges(float4 color, float distFromCenter)
{
	float distFromEdge = max(distFromCenter - (Radius - BlurredEdge), 0);
	float lum = saturate(1 - distFromEdge / BlurredEdge);
	return color * lum;
}

struct VSInput
{
	float3 pos : POSITION0;
	float3 normal : NORMAL0;
	float3 center : POSITION1;
	float normStepFromNoteStart : POSITION2;
	float2 texCoords : TEXCOORD0;
};

struct VSOutput
{
	float4 pos : POSITION0;
	float3 normal : TEXCOORD1;
	float3 center : POSITION1;
	float3 rawPos : POSITION2;
	float normStepFromNoteStart : POSITION3;
	float2 texCoords : TEXCOORD0;
};

void VS(in VSInput IN, out VSOutput OUT)
{
	OUT.normal = IN.normal;
	OUT.center = IN.center;
	//OUT.center.x -= SongPos;
	//IN.pos.x -= SongPos;
	OUT.rawPos = IN.pos.xyz;
	OUT.normStepFromNoteStart = IN.normStepFromNoteStart;
	//OUT.pos.xy -= 0.5;
	OUT.texCoords = IN.texCoords - TexScrollOffset;

	OUT.pos = float4(IN.pos.xy, 0, 1);
	OUT.pos.xyz += PosOffset;
	OUT.pos.x -= SongPos;
	OUT.pos = mul(OUT.pos, VpMat);
	// Viewport adjustment.
	//OUT.pos.xy /= ViewportSize;
	//OUT.pos.xy *= float2(2, -2);
	//OUT.pos.xy -= float2(1, -1);
}

void PS(out float4 color : COLOR0, in VSOutput IN)
{
	float3 lightingNormal;
	if (Style == 1) //Ribbon
	{
		if (IN.normal.x > 0)
			IN.normal *= -1;
		if (abs(IN.normal.x) < 0.000001f)
			IN.normal.y = 1;
		lightingNormal = lerp(IN.normal, float3(0, 0, 1), abs(IN.normal.x));
	}
	else
		lightingNormal = float3(0, 0, 1);
				
	lightingNormal = normalize(lightingNormal);
	//color = float4(Color.rgb, 1);
	color = Color;
	color.rgb *= tex2D(TextureSampler, IN.texCoords);
	//color.a *= (Color.r + Color.g + Color.b) / 3;
		
	float3 normal = normalize(IN.normal);
	float3 tPos = IN.rawPos - IN.center;
	float normDistFromEdge = dot(tPos, normal) / Radius * 0.5f + 0.5f;
	float2 normPos = float2(IN.normStepFromNoteStart, normDistFromEdge);
	IN.rawPos.x -= SongPos;
	color = modulate(normPos, color, lightingNormal, IN.rawPos);
}

float ClipPercent;
float ArrowLength;
float3 Side1Normal;
float3 Side2Normal;
float3 ArrowDir;
float3 ArrowStart;
float3 ArrowEnd;
float4 HlColor;
float DistToCenter;

void ArrowAreaPS(out float4 color : COLOR0, in VSOutput IN)
{
	//color = float4(1,1,1,1);
	//return;
	
	//float distFromStart = dot(rawPos - ArrowStart, ArrowDir);
	//clip(distFromStart / ArrowLength - ClipPercent);
	//color = lerp(HlColor, Color, ClipPercent);
	float distFromBottom = abs(dot(IN.rawPos - ArrowStart, ArrowDir));
	float distFromSide1 = abs(dot(IN.rawPos - ArrowEnd, Side1Normal));
	float distFromSide2 = abs(dot(IN.rawPos - ArrowEnd, Side2Normal));
	float dist = min(distFromBottom, distFromSide1);
	dist = min(dist, distFromSide2);
	float normDistFromBorder = dist / DistToCenter;
	//color = shadeHlObject(dist, DistToCenter);
	float lum;
	if (Border)
	{
		float distFromInnerEdge = dist - ClipPercent * DistToCenter;
		lum = saturate(distFromInnerEdge / BlurredEdge);
		//float lum2 = saturate(1 - abs(distFromInnerEdge / BlurredEdge));
		float lum2 = saturate(1 - abs((dist - BlurredEdge) / BlurredEdge));
		lum = max(lum, lum2);

		color = HlColor * lum;
	}
	else
	{
		if (normDistFromBorder > ClipPercent)
			color = HlColor;
		else
			color = Color;
		lum = saturate(dist / BlurredEdge);
		color *= lum;
	}
	//
	color.a = 0;
	//color = float4(0, 0, 1, 1);
	//color = float4(rawPos, 1);
}

//void ArrowBorderPS(out float4 color : COLOR0, float3 normal : NORMAL0, float3 arrowDir : NORMAL1, float3 arrowStart : POSITION1, float3 rawPos : POSITION2)
//{
//	float distFromBottom = abs(dot(rawPos - ArrowStart, ArrowDir));
//	float distFromSide1 = abs(dot(rawPos - ArrowEnd, Side1Normal));
//	float distFromSide2 = abs(dot(rawPos - ArrowEnd, Side2Normal));
//	float dist = min(distFromBottom, distFromSide1);
//	dist = min(dist, distFromSide2);
//
//	float arrowBorderBlur = 2;
//	float lum = saturate(1 - dist / arrowBorderBlur);
//	color = Color * lum;
//}

//void CirclePS(out float4 color : COLOR0, float3 rawPos : POSITION2)
void CirclePS(out float4 color : COLOR0, in VSOutput IN)
{
	float lum = 0;
	float3 tPos = IN.rawPos - WorldPos;
	float distFromCenter = length(tPos);
	float sgnDistFromEdge = distFromCenter - (HlSize - BlurredEdge);
	float distFromEdge = abs(sgnDistFromEdge);
	
	if (Border)
	{
		lum = saturate(1 - distFromEdge / BlurredEdge);
		float distFromInnerEdge = max(distFromCenter - (InnerHlSize - BlurredEdge), 0);
		float lum2 = saturate(1 - distFromInnerEdge / BlurredEdge);
		lum = max(lum, lum2);
		color = HlColor * lum;
	}
	else
	{
		if (distFromCenter < InnerHlSize)
			color = HlColor;
		else
			color = Color;
		lum = 1 - saturate(sgnDistFromEdge / BlurredEdge);
		color *= lum;
	}
	
	color.a = 0;
	//color.rgb *= saturate(pow(distFromCenter*0.0015, 30));
	//color.rgb = rawPos;
}

float4 shadeHlObject(float sgnDistFromEdge, float distFromCenter)
{
	float4 color;
	float lum;
	float distFromEdge = abs(sgnDistFromEdge);
	if (Border)
	{
		lum = saturate(1 - distFromEdge / BlurredEdge);
		float distFromInnerEdge = max(distFromEdge - distFromCenter * ClipPercent - BlurredEdge, 0);
		float lum2 = saturate(1 - distFromInnerEdge / BlurredEdge);
		lum = max(lum, lum2);
		color = HlColor * lum;
	}
	else
	{
		if (distFromCenter < InnerHlSize)
			color = HlColor;
		else
			color = Color;
		lum = 1 - saturate(sgnDistFromEdge / BlurredEdge);
		color *= lum;
	}
	color.a = 0;
	return color;
}

technique Line
{
	pass
	{
		CullMode = None;
		VertexShader = compile vs_5_0 VS();
		PixelShader  = compile ps_5_0 PS();
	}
}

technique Arrow
{
	pass Area
	{
		CullMode = None;
		VertexShader = compile vs_5_0 VS();
		PixelShader  = compile ps_5_0 ArrowAreaPS();
	}
	/*pass Border
	{
		CullMode = None;
		VertexShader = compile vs_5_0 VS();
		PixelShader  = compile ps_5_0 ArrowBorderPS();
	}*/
}

technique Circle
{
	pass
	{
		CullMode = None;
		VertexShader = compile vs_5_0 VS();
		PixelShader  = compile ps_5_0 CirclePS();
	}
}