#include "notestyle.fx"

// Input parameters

float Radius;
int LineType;
float3 WorldPos;
float HlSize;
float InnerHlSize;
bool Border;
float2 TessellationViewportSize;

struct PatchVSInput
{
	float3 center : POSITION0;
	float4 texCoords : TEXCOORD0;
	float4 texCoords2 : TEXCOORD1;
	float2 patchData : TEXCOORD2;
	float4 endpointTangents : TEXCOORD3;
};

struct PatchControlPoint
{
	float3 center : BEZIERPOS;
	float4 texCoords : TEXCOORD0;
	float4 texCoords2 : TEXCOORD1;
	float2 patchData : TEXCOORD2;
	float4 endpointTangents : TEXCOORD3;
};

struct PatchConstants
{
	float edgeTess[4] : SV_TessFactor;
	float insideTess[2] : SV_InsideTessFactor;
};

struct LineOutput
{
	float4 pos : SV_POSITION;
	float3 normal : TEXCOORD1;
	float3 center : POSITION1;
	float3 rawPos : POSITION2;
	float2 normPos : POSITION3;
	float2 worldSize : POSITION4;
	float2 texCoords : TEXCOORD0;
	float2 texCoords2 : TEXCOORD2;
};

struct LegacyVSInput
{
	float3 pos : POSITION0;
	float3 normal : NORMAL0;
	float3 center : POSITION1;
	float2 normPos : POSITION2;
	float2 texCoords : TEXCOORD0;
	float2 texCoords2 : TEXCOORD1;
};

LineOutput LegacyVS(LegacyVSInput input)
{
	// This is the pre-hardware-tessellation path. The CPU-expanded vertices stay
	// resident beside the patches so Ctrl+Shift+H can switch without rebuilding.
	float3 center = input.center;
	center.x *= VertWidthScale;

	float3 normal = input.normal;
	normal.y *= VertWidthScale;
	normal = normalize(normal);

	float side = input.normPos.y * 2 - 1;
	float3 offset = LineType == 1 ? float3(side, 0, 0) : normal * side;
	offset *= Radius;

	float3 rawPos = center + offset;
	LineOutput output;
	output.normal = normal;
	output.center = center;
	output.rawPos = rawPos;
	output.normPos = input.normPos;
	// The legacy vertex format never carried the note's world length.
	output.worldSize = float2(0, Radius * 2);
	output.pos = float4(rawPos.xy, 0, 1);
	output.pos = wvpTransform(output.pos, 1);
	output.texCoords = input.texCoords - TexScrollOffset;
	output.texCoords2 = input.texCoords2 - TexScrollOffset2;
	return output;
}

PatchControlPoint PatchVS(PatchVSInput input)
{
	PatchControlPoint output;
	output.center = input.center;
	output.texCoords = input.texCoords;
	output.texCoords2 = input.texCoords2;
	output.patchData = input.patchData;
	output.endpointTangents = input.endpointTangents;
	return output;
}

float2 projectToPixels(float3 center)
{
	center.x *= VertWidthScale;
	float4 clipPosition = float4(center, 1);
	clipPosition = wvpTransform(clipPosition, 1);
	float safeW = abs(clipPosition.w) < 0.00001f
		? (clipPosition.w < 0 ? -0.00001f : 0.00001f)
		: clipPosition.w;
	return clipPosition.xy / safeW * TessellationViewportSize * 0.5f;
}

float distanceToLine(float2 position, float2 start, float2 end)
{
	float2 chord = end - start;
	float lineLength = length(chord);
	if (lineLength < 0.0001f)
		return length(position - start);
	return abs(chord.x * (start.y - position.y) - (start.x - position.x) * chord.y) / lineLength;
}

PatchConstants PatchConstantFunction(InputPatch<PatchControlPoint, 4> patch)
{
	float2 p0 = projectToPixels(patch[0].center);
	float2 p1 = projectToPixels(patch[1].center);
	float2 p2 = projectToPixels(patch[2].center);
	float2 p3 = projectToPixels(patch[3].center);
	float flatness = max(distanceToLine(p1, p0, p3), distanceToLine(p2, p0, p3));
	float segments = clamp(ceil(sqrt(max(flatness, 0) / 0.25f)), 1, 64);

	PatchConstants output;
	// u follows the curve and v crosses its width.  The v==0 and v==1 edges
	// therefore receive the adaptive factor; only one segment is needed across.
	output.edgeTess[0] = 1;
	output.edgeTess[1] = segments;
	output.edgeTess[2] = 1;
	output.edgeTess[3] = segments;
	output.insideTess[0] = segments;
	output.insideTess[1] = 1;
	return output;
}

[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(4)]
[patchconstantfunc("PatchConstantFunction")]
[maxtessfactor(64.0)]
PatchControlPoint HS(InputPatch<PatchControlPoint, 4> patch,
	uint controlPointId : SV_OutputControlPointID)
{
	return patch[controlPointId];
}

float3 evaluatePosition(const OutputPatch<PatchControlPoint, 4> patch, float t)
{
	float s = 1 - t;
	return patch[0].center * (s * s * s)
		+ patch[1].center * (3 * s * s * t)
		+ patch[2].center * (3 * s * t * t)
		+ patch[3].center * (t * t * t);
}

float3 evaluateTangent(const OutputPatch<PatchControlPoint, 4> patch, float t)
{
	float s = 1 - t;
	return (patch[1].center - patch[0].center) * (3 * s * s)
		+ (patch[2].center - patch[1].center) * (6 * s * t)
		+ (patch[3].center - patch[2].center) * (3 * t * t);
}

float4 evaluateTexCoords(const OutputPatch<PatchControlPoint, 4> patch, float t, bool second)
{
	float s = 1 - t;
	float4 p0 = second ? patch[0].texCoords2 : patch[0].texCoords;
	float4 p1 = second ? patch[1].texCoords2 : patch[1].texCoords;
	float4 p2 = second ? patch[2].texCoords2 : patch[2].texCoords;
	float4 p3 = second ? patch[3].texCoords2 : patch[3].texCoords;
	return p0 * (s * s * s)
		+ p1 * (3 * s * s * t)
		+ p2 * (3 * s * t * t)
		+ p3 * (t * t * t);
}

float2 evaluatePatchData(const OutputPatch<PatchControlPoint, 4> patch, float t)
{
	float s = 1 - t;
	return patch[0].patchData * (s * s * s)
		+ patch[1].patchData * (3 * s * s * t)
		+ patch[2].patchData * (3 * s * t * t)
		+ patch[3].patchData * (t * t * t);
}

[domain("quad")]
LineOutput DS(PatchConstants constants, float2 domainLocation : SV_DomainLocation,
	const OutputPatch<PatchControlPoint, 4> patch)
{
	float curveT = domainLocation.x;
	float sideT = domainLocation.y;
	float side = sideT * 2 - 1;
	float3 center = evaluatePosition(patch, curveT);
	float3 tangent = evaluateTangent(patch, curveT);
	// Each connected patch uses the preceding patch's final cross-section at the
	// shared endpoint. This also spans duplicate MIDI notes, whose zero-duration
	// curve interval is skipped by the CPU builder.
	if (curveT <= 0.000001f)
		tangent.xy = patch[0].endpointTangents.xy;
	else if (curveT >= 0.999999f)
		tangent.xy = patch[0].endpointTangents.zw;
	center.x *= VertWidthScale;
	tangent.x *= VertWidthScale;
	float3 normal = normalize(float3(-tangent.y, tangent.x, 0));

	float3 offset;
	if (LineType == 1)
	{
		// A horizontal ribbon otherwise collapses to a zero-area surface.  Match
		// the old one-pixel LineList fallback with a sub-pixel vertical span.
		float horizontalFallback = saturate(1 - abs(normal.x) * 1000) * 0.5f;
		offset = float3(side * Radius, side * horizontalFallback, 0);
	}
	else
		offset = normal * side * Radius;

	float3 rawPos = center + offset;
	float4 packedTexCoords = evaluateTexCoords(patch, curveT, false);
	float4 packedTexCoords2 = evaluateTexCoords(patch, curveT, true);
	float2 patchData = evaluatePatchData(patch, curveT);

	LineOutput output;
	output.normal = normal;
	output.center = center;
	output.rawPos = rawPos;
	output.normPos = float2(patchData.x, sideT);
	output.worldSize = float2(patchData.y * VertWidthScale, Radius * 2);
	output.pos = float4(rawPos.xy, 0, 1);
	output.pos = wvpTransform(output.pos, 1);
	output.texCoords = lerp(packedTexCoords.xy, packedTexCoords.zw, sideT) - TexScrollOffset;
	output.texCoords2 = lerp(packedTexCoords2.xy, packedTexCoords2.zw, sideT) - TexScrollOffset2;
	return output;
}

float4 PS(in LineOutput IN) : COLOR0
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
    float4 color = getPixelColor(Color, IN.texCoords, IN.texCoords2);
		
	float3 normal = normalize(IN.normal);
	float3 tPos = IN.rawPos - IN.center;
	float normDistFromEdge = dot(tPos, normal) / Radius * 0.5f + 0.5f;
	float2 normPos = IN.normPos;
	IN.rawPos.x -= SongPos;
	color = modulate(normPos, IN.worldSize, color, lightingNormal, IN.rawPos);
	color = blurEdges(color, normPos.y);
    return color;
}


technique LineHardware
{
	pass
	{
		VertexShader = compile vs_5_0 PatchVS();
		HullShader = compile hs_5_0 HS();
		DomainShader = compile ds_5_0 DS();
		PixelShader  = compile ps_5_0 PS();
	}   

}

technique LineLegacy
{
	pass
	{
		VertexShader = compile vs_5_0 LegacyVS();
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
		float size = HlSize;
		if (InnerHlSize > 0)
			size = InnerHlSize;
		if (distFromCenter < InnerHlSize)
			color = IN.hlColor;
		else
			color = IN.color;

		//lum = 1 - saturate(sgnDistFromEdge / (BlurredEdge));
		lum = saturate((size - distFromCenter) / size);
		lum = pow(lum, 0.75f);
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
