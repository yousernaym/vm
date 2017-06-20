#include "notestyle.fx"

// Input parameters

float Radius;
float FadeoutFromCenter;
float ShapePower;
int Style;
float3 WorldPos;
float HlSize;
float InnerHlSize;
bool Border;

//Helper functions------------------
float4 shadeHlObject(float sgnDistFromEdge, float distFromCenter);

void initPixel(inout float4 color, inout float3 normal, inout float3 normal2, inout float3 tPos, inout float distFromCenter, inout float distSign, inout float height, inout float normDistFromCenter, inout float2 texCoords, float3 rawPos, float3 center, float ShapePower, float fadeoutFromCenter)
{
	color = float4(Color.rgb,1);
	normal = normalize(normal);
	normal2 = normalize(normal2);
	tPos = rawPos - center;
	
	distFromCenter = dot(tPos, normal);
	distSign = sign(distFromCenter);
	distFromCenter = abs(distFromCenter);
	
	//float distFromCenter = length(tPos);
	
	if (fadeoutFromCenter == 0)
	{
		height = 1;
		normDistFromCenter = 0;
	}
	else
	{
		normDistFromCenter = distFromCenter / fadeoutFromCenter;
		height = saturate(1 - pow(abs(normDistFromCenter), ShapePower));  //Use abs to prevent compiler warning
	}
	//texCoords.x = (rawPos.x - normal.x * distFromCenter * distSign) / TexSize.x;
	//texCoords.y = normDistFromCenter * distSign * 0.5f + 0.5f;
}

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
	float3 normal2 : NORMAL1;
	float3 center : POSITION1;
	float2 texCoords : TEXCOORD0;
};

struct VSOutput
{
	float4 pos : POSITION0;
	float3 normal : TEXCOORD1;
	float3 normal2 : TEXCOORD2;
	float3 center : POSITION1;
	float3 rawPos : POSITION2;
	float2 texCoords : TEXCOORD0;
};

void VS(in VSInput IN, out VSOutput OUT)				
{
	OUT.normal = IN.normal;
	OUT.normal2 = IN.normal2;
	OUT.center = IN.center;
	
	OUT.rawPos = IN.pos.xyz;
	//OUT.pos.xy -= 0.5;
	// Compute the texture coordinate.
	//OUT.texCoords = OUT.rawPos.xy / TexSize;
	OUT.texCoords = IN.texCoords;

	OUT.pos = float4(IN.pos.xyz, 1);
	OUT.pos.z = -2.412;
	OUT.pos = mul(OUT.pos, WvpMat);
	// Viewport adjustment.
	//OUT.pos.xy /= ViewportSize;
	//OUT.pos.xy *= float2(2, -2);
	//OUT.pos.xy -= float2(1, -1);

	// Apply the matrix transform.
	//OUT.pos = mul(IN.pos.xyz, transpose(MatrixTransform));
}


// Pixel shader for rendering sprites (shared between Windows and Xbox).
void SimplePS(out float4 color : COLOR0, in VSOutput IN)
{
	//color = float4(IN.normal,1);

	//return;
	float3 tPos; float distFromCenter; float distSign; float height; float normDistFromCenter;
	initPixel(color, IN.normal, IN.normal2, tPos, distFromCenter, distSign, height, normDistFromCenter, IN.texCoords, IN.rawPos, IN.center, ShapePower, FadeoutFromCenter);
		
	color.rgb *= height;
	color.rgb *= tex2D(TextureSampler, IN.texCoords);
	color = blurEdges(color, distFromCenter);
	//color.z = tPos.x;
	//color = float4(IN.normal.x, IN.rawPos.x, 1, 1);
	//color = float4(0, tPos.y*0.1, 0, 1);
}

void LightingPS(out float4 color : COLOR0, in VSOutput IN)
{
	float3 tPos; float distFromCenter; float distSign; float height; float normDistFromCenter;
	initPixel(color, IN.normal, IN.normal2, tPos, distFromCenter, distSign, height, normDistFromCenter, IN.texCoords, IN.rawPos, IN.center, ShapePower, FadeoutFromCenter);

	float3 lightingNormal;
	
	if (Style == 1) //Ribbon
	{
		if (IN.normal2.x > 0)
			IN.normal2 *= -1;
		if (abs(IN.normal2.x) < 0.000001f)
		{
			//normal2.x = 0;
			IN.normal2.y = 1;
		}
		lightingNormal = lerp(IN.normal2, float3(0,0,1), abs(IN.normal2.x));
		//lightingNormal = normal2 + float3(0,0,1);
	}
	else if (Style == 2) //Tube, formerly known as Clamped Tube
	{
		lightingNormal = lerp(IN.normal*distSign, float3(0,0,1), height);
	}
	else if (Style == 3) //Old Tube
	{
		lightingNormal = IN.normal*distSign * normDistFromCenter;
		lightingNormal.z = height;
		//lightingNormal = float3(0,0,1);
	}
				
	lightingNormal = normalize(lightingNormal);
	
	float lum = clamp(dot(LightDir, lightingNormal), AmbientLum, 1);
	//lum = saturate(dot(light, lightingNormal) + AmbientLum);
	
	float3 lightReflection = -reflect(LightDir, lightingNormal);
	
	color.rgb *= lum;
	float3 viewVec = normalize(SpecCamPos - IN.rawPos);
	color.rgb *= tex2D(TextureSampler, IN.texCoords);
	color.rgb += pow(saturate(dot(lightReflection, viewVec)), SpecPower) * SpecAmount;
	color = blurEdges(color, distFromCenter);
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

technique Simple
{
	pass
	{
		CullMode = None;
		VertexShader = compile vs_5_0 VS();
		PixelShader  = compile ps_5_0 SimplePS();
	}
}

technique Lighting
{
	pass
	{
		CullMode = None;
		VertexShader = compile vs_5_0 VS();
		PixelShader  = compile ps_5_0 LightingPS();
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