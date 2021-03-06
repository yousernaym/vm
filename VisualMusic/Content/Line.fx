#include "notestyle.fx"

// Input parameters

float Radius;
int LineType;
float3 WorldPos;
float HlSize;
float InnerHlSize;
bool Border;

struct VSInput
{
	float3 pos : POSITION0;
	float3 normal : NORMAL0;
	float3 center : POSITION1;
	float2 normPos : POSITION2;
	float2 texCoords : TEXCOORD0;
};

struct VSOutput
{
	float4 pos : POSITION0;
	float3 normal : TEXCOORD1;
	float3 center : POSITION1;
	float3 rawPos : POSITION2;
	float2 normPos : POSITION3;
	float2 worldSize : POSITION4;
	float2 texCoords : TEXCOORD0;
};

void VS(in VSInput IN, out VSOutput OUT)
{
	OUT.normal = IN.normal;
	OUT.center = IN.center;
	OUT.rawPos = IN.pos.xyz;
	OUT.normPos = IN.normPos;
	OUT.pos = float4(IN.pos.xy, 0, 1);
	OUT.pos = wvpTransform(OUT.pos, VertWidthScale);
	OUT.texCoords = IN.texCoords - TexScrollOffset;
}

float4 PS(in VSOutput IN) : COLOR0
{
	float3 lightingNormal;
	if (LineType == 1) //Ribbon
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
    float4 color = getPixelColor(Color, IN.texCoords);
		
	float3 normal = normalize(IN.normal);
	float3 tPos = IN.rawPos - IN.center;
	float normDistFromEdge = dot(tPos, normal) / Radius * 0.5f + 0.5f;
	float2 normPos = IN.normPos;
	IN.rawPos.x -= SongPos;
	color = modulate(normPos, IN.worldSize, color, lightingNormal, IN.rawPos);
	color = blurEdges(color, normPos.y);
    return color;
}


technique Line
{
	pass
	{
     	VertexShader = compile vs_5_0 VS();
		PixelShader  = compile ps_5_0 PS();
	}   

}

//Highlights------------------------------

struct HlVSInput
{
	float3 pos : POSITION0;
};

struct HlVSOutput
{
	float4 pos : POSITION0;
	float3 rawPos : POSITION1;
    float4 color : COLOR0;
    float4 hlColor : COLOR1;
};

void HlVS(in HlVSInput IN, out HlVSOutput OUT)
{
	OUT.rawPos = IN.pos.xyz;
	OUT.pos = float4(IN.pos.xy, 0, 1);
	OUT.pos = wvpTransform(OUT.pos, 1);
    OUT.color = HslaToRgba(Color);
    OUT.hlColor = HslaToRgba(HlColor);
}

float ClipPercent;
float ArrowLength;
float3 Side1Normal;
float3 Side2Normal;
float3 ArrowDir;
float3 ArrowStart;
float3 ArrowEnd;
float DistToCenter;

void ArrowPS(out float4 color : COLOR0, in HlVSOutput IN)
{
	float distFromBottom = abs(dot(IN.rawPos - ArrowStart, ArrowDir));
	float distFromSide1 = abs(dot(IN.rawPos - ArrowEnd, Side1Normal));
	float distFromSide2 = abs(dot(IN.rawPos - ArrowEnd, Side2Normal));
	float dist = min(distFromBottom, distFromSide1);
	dist = min(dist, distFromSide2);
	float normDistFromBorder = dist / DistToCenter;

	float lum;
	if (Border)
	{
		float distFromInnerEdge = dist - ClipPercent * DistToCenter;
		lum = saturate(distFromInnerEdge / BlurredEdge);
		//float lum2 = saturate(1 - abs(distFromInnerEdge / BlurredEdge));
		float lum2 = saturate(1 - abs((dist - BlurredEdge) / BlurredEdge));
		lum = max(lum, lum2);

		color = IN.hlColor * lum;
	}
	else
	{
		if (normDistFromBorder > ClipPercent)
			color = IN.hlColor;
		else
			color = IN.color;
		lum = saturate(dist / BlurredEdge);
		color *= lum;
	}
	color.a = 0;
    color *= SongFade;
}

void CirclePS(out float4 color : COLOR0, in HlVSOutput IN)
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
		color = IN.hlColor * lum;
	}
	else
	{
		if (distFromCenter < InnerHlSize)
			color = IN.hlColor;
		else
			color = IN.color;
		lum = 1 - saturate(sgnDistFromEdge / BlurredEdge);
		color *= lum;
	}

	color.a = 0;
    color *= SongFade;
}

technique Arrow
{
    pass
    {
        VertexShader = compile vs_5_0 HlVS();
        PixelShader  = compile ps_5_0 ArrowPS();
    }
}

technique Circle
{
	pass
	{
		VertexShader = compile vs_5_0 HlVS();
		PixelShader  = compile ps_5_0 CirclePS();
	}
}