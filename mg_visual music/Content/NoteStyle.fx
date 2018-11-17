float SongFade;
float4x4 VpMat;
float2 ViewportSize;
texture Texture;
bool UseTexture;
bool TexColBlend;
float2 ProjScale;
float SongPos;
float2 TexScrollOffset;
float VertWidthScale;
float TexWidthScale;
float4 HlColor;

sampler  TextureSampler = sampler_state
{
	texture = <Texture>;
};
float4 Color;
float BlurredEdgePixels = 2;
float BlurredEdge;
float3 LightDir = normalize(float3(1, 1, 1));
float4 AmbientColor;
float4 DiffuseColor;
float4 SpecColor;
float SpecPower;
float4 LightFilter;
float3 CamPos;
float3 PosOffset;
bool DiscardAtOnce;

#define ModEntriesCount 5

float2 Origin[ModEntriesCount];
bool XOriginEnable[ModEntriesCount];
bool YOriginEnable[ModEntriesCount];
int CombineXY[ModEntriesCount];
bool SquareAspect[ModEntriesCount];
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

struct ModEntry
{
	float2 Origin;
	bool XOriginEnable;
	bool YOriginEnable;
	int CombineXY;
	bool SquareAspect;
	bool ColorDestEnable;
	bool AlphaDestEnable;
	bool AngleDestEnable;
	float4 ColorDest;
	float AngleDest;
	float Start;
	float Stop;
	float FadeIn;
	float FadeOut;
	float Power;
	bool DiscardAfterStop;
	bool Invert;
};

//Struct arrays cannot yet be passed to shaders in Monogame, so pass arrays of individual ellements and copy them to the struct here
void fillModEntryStruct(out ModEntry entry, int index)
{
	entry.Origin = Origin[index];
	entry.XOriginEnable = XOriginEnable[index];
	entry.YOriginEnable = YOriginEnable[index];
	entry.CombineXY = CombineXY[index];
	entry.SquareAspect = SquareAspect[index];
	entry.ColorDestEnable = ColorDestEnable[index];
	entry.AlphaDestEnable = AlphaDestEnable[index];
	entry.AngleDestEnable = AngleDestEnable[index];
	entry.ColorDest = ColorDest[index];
	entry.AngleDest = AngleDest[index];
	entry.Start = Start[index];
	entry.Stop = Stop[index];
	entry.FadeIn = FadeIn[index];
	entry.FadeOut = FadeOut[index];
	entry.Power = Power[index];
	entry.DiscardAfterStop = DiscardAfterStop[index];
	entry.Invert = Invert[index];
	
}

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
	color *= saturate(dot(LightDir, normal)) * DiffuseColor + AmbientColor;
	float3 lightReflection = -reflect(LightDir, normal);
	float3 viewVec = normalize(CamPos - worldPos);
	//if (any(color))
		color += pow(saturate(dot(lightReflection, viewVec)), SpecPower) * SpecColor;
    color *= LightFilter;
    return color * SongFade;
}

float getInterpolant(ModEntry modEntry, float2 normPos, float2 noteSize, out float3 destNormalDir, out bool discardFade)
{
	discardFade = false;
	float2 transformedNormPos = normPos - modEntry.Origin;
	float2 distToEdgeFromOrigin = modEntry.Origin;
	if (transformedNormPos.x < 0)
		transformedNormPos.x /= modEntry.Origin.x;
	else
	{
		transformedNormPos.x /= 1 - modEntry.Origin.x;
		distToEdgeFromOrigin = 1 - modEntry.Origin.x;
	}
	if (transformedNormPos.y < 0)
        transformedNormPos.y /= modEntry.Origin.y;
	else
	{
		transformedNormPos.y /= 1 - modEntry.Origin.y;
		distToEdgeFromOrigin = 1 - modEntry.Origin.y;
	}
	float2 normDistFromOrigin = abs(transformedNormPos);
	distToEdgeFromOrigin = abs(distToEdgeFromOrigin * noteSize);
	
	float2 distFromOrigin = normDistFromOrigin * distToEdgeFromOrigin;
	if (modEntry.SquareAspect && distToEdgeFromOrigin.x > distToEdgeFromOrigin.y)
	{
        if (distToEdgeFromOrigin.y == 0)
            normDistFromOrigin.x = 0;
		else
        {
            float ratio = distToEdgeFromOrigin.x / distToEdgeFromOrigin.y;
            float normDistFromEdgeX = min((1 - normDistFromOrigin.x) * ratio, 1); //1:1 aspect by increasing x dist from edge to match y dist from edge
            normDistFromOrigin.x = 1 - normDistFromEdgeX;
			//normDistFromOrigin.y = normDistFromOrigin.y / ratio; //1:1 aspect by reducing y dist from origin to match x dist from origin
        }
    }
	transformedNormPos = normDistFromOrigin * sign(transformedNormPos);
	float interpolant = 0;
	destNormalDir = float3(0, 0, 0);

	if (modEntry.XOriginEnable && modEntry.YOriginEnable)
	{
		if (modEntry.CombineXY == CombineXY_Max)
		{
			if (normDistFromOrigin.x > normDistFromOrigin.y)
				destNormalDir.x = transformedNormPos.x;
			else
				destNormalDir.y = transformedNormPos.y;
			interpolant = max(normDistFromOrigin.x, normDistFromOrigin.y);
		}
		else if (modEntry.CombineXY == CombineXY_Min)
		{
			if (normDistFromOrigin.x < normDistFromOrigin.y)
				destNormalDir.x = transformedNormPos.x;
			else
				destNormalDir.y = transformedNormPos.y;
			interpolant = min(normDistFromOrigin.x, normDistFromOrigin.y);
		}
		else //Both x and y is used
		{
			destNormalDir = float3(transformedNormPos, 0);
			if (modEntry.CombineXY == CombineXY_Add)
				interpolant = normDistFromOrigin.x + normDistFromOrigin.y;
			else if (modEntry.CombineXY == CombineXY_Length)
				interpolant = pow(normDistFromOrigin.x, 2) + pow(normDistFromOrigin.y, 2);
		}
	}
	else if (modEntry.XOriginEnable)
	{
		destNormalDir.x = transformedNormPos.x;
		interpolant = normDistFromOrigin.x;
	}
	else if (modEntry.YOriginEnable)
	{
		destNormalDir.y = transformedNormPos.y;
		interpolant = normDistFromOrigin.y;
	}
	else
		return 0;

    if (!any(destNormalDir))
       destNormalDir.x = 1; //Pick a random direction with z = 0
	destNormalDir = normalize(destNormalDir);
		
	//Outside Start - Stop?
	bool discardBeforeStart = modEntry.Invert && modEntry.DiscardAfterStop;
	//bool discardBeforeStart = modEntry.DiscardAfterStop;
	bool discardAfterStop = !modEntry.Invert && modEntry.DiscardAfterStop;
	if (interpolant < modEntry.Start)
		return discardBeforeStart ? -1 : 0;
	if (interpolant > modEntry.Stop)
		return discardAfterStop ? -1 : 0;
			
	interpolant -= modEntry.Start;
	interpolant /= modEntry.Stop - modEntry.Start;
	
	if (interpolant > modEntry.FadeIn && 1 - interpolant > modEntry.FadeOut)
		return 1;
	
	float fadeInInterpolant;
	if (modEntry.FadeIn > 0 && interpolant < modEntry.FadeIn)
	{
		fadeInInterpolant = interpolant / modEntry.FadeIn;
		if (discardBeforeStart)
			discardFade = true;
	}
	else
		fadeInInterpolant = 1;

	float fadeOutInterpolant;
	if (modEntry.FadeOut > 0 && 1 - interpolant < modEntry.FadeOut)
	{
		fadeOutInterpolant = (1 - interpolant) / modEntry.FadeOut;
		if (discardAfterStop)
			discardFade = true;
	}
	else
		fadeOutInterpolant = 1;
	interpolant = min(fadeOutInterpolant, fadeInInterpolant);
	
	interpolant = pow(saturate(interpolant), modEntry.Power);
	return interpolant;
}
float4 modulate(float2 normPos, float2 noteSize, float4 sourceColor, float3 sourceNormal, float3 worldPos)
{
    float4 result = float4(1, 1, 1, 1);
	float3 destNormal = float3(0,0,0);
	
	float gradLength = max(length(ddx(normPos)), length(ddy(normPos)));
	for (int i = 0; i < ActiveModEntries; i++)
	{
		if (ColorDestEnable[i] || AlphaDestEnable[i] || AngleDestEnable[i])
		{
			ModEntry modEntry;
			fillModEntryStruct(modEntry, i);
			modEntry.FadeOut = max(gradLength * 3, modEntry.FadeOut);
            modEntry.FadeOut = max(0.05f, modEntry.FadeOut);

			if (modEntry.Start > 0)
				modEntry.FadeIn = max(gradLength * 3, modEntry.FadeIn);
			modEntry.FadeIn = min(1 - modEntry.FadeOut, modEntry.FadeIn);
					
			float3 destNormalDir;
			bool discardFade;
			float interpolant = getInterpolant(modEntry, normPos, noteSize, destNormalDir, discardFade);
			
            if (DiscardAtOnce && (discardFade || interpolant < 0))
                discard;
			if (interpolant < 0) //Discard after stop
			{
				result = 0;
				//result = blurEdges(result, -interpolant);
				continue;
			}
           	if (Invert[i])
				interpolant = 1 - interpolant;
			if (ColorDestEnable[i])
			{
				result.rgb = lerp(discardFade ? ColorDest[i].rgb : result.rgb, ColorDest[i].rgb, interpolant);
			}
			if (AlphaDestEnable[i])
			{
				result.a = lerp(discardFade ? ColorDest[i].a : result.a, ColorDest[i].a, interpolant);
			}
			if (AngleDestEnable[i])
			{
				float angle = AngleDest[i];
				if (!discardFade)
					angle *= interpolant; //lerp from 0 to AngleDest
				
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
    float4 finalCol = result * sourceColor;
	if (any(result))
		finalCol.rgb = calcLighting(finalCol.rgb, normalize(destNormal), worldPos);
    return finalCol;
}

float4 HslaToRgba(float4 hsla)
{
    float v;
    float h = hsla.x;
    float s = hsla.y;
    float l = hsla.z;
    double r, g, b;
    r = g = b = l; // default to gray
    v = (l <= 0.5) ? (l * (1.0 + s)) : (l + s - l * s);
    if (v > 0)
    {
        float m;
        float sv;
        int sextant;
        float fract, vsf, mid1, mid2;
        m = l + l - v;
        sv = (v - m) / v;
        h *= 6.0;
        sextant = (int) h;
        fract = h - sextant;
        vsf = v * sv * fract;
        mid1 = m + vsf;
        mid2 = v - vsf;
        switch (sextant)
        {
            case 0:
                r = v;
                g = mid1;
                b = m;
                break;
            case 1:
                r = mid2;
                g = v;
                b = m;
                break;
            case 2:
                r = m;
                g = v;
                b = mid1;
                break;
            case 3:
                r = m;
                g = mid2;
                b = v;
                break;
            case 4:
                r = mid1;
                g = m;
                b = v;
                break;
            case 5:
                r = v;
                g = m;
                b = mid2;
                break;
        }
    }
    return float4(r, g, b, hsla.a);;
}


float4 RgbaToHsla(float4 rgba)
{
    float r = rgba.x;
    float g = rgba.y;
    float b = rgba.z;
    float maxValue = max(r, g);
    maxValue = max(maxValue, b);
    float minValue = min(r, g);
    minValue = min(minValue, b);
    float h, s, l = (maxValue + minValue) / 2;

    if (maxValue == minValue)
    {
        h = s = 0; // achromatic
    }
    else
    {
        float d = maxValue - minValue;
        s = l > 0.5 ? d / (2 - maxValue - minValue) : d / (maxValue + minValue);
        if (maxValue == r)
            h = (g - b) / d + (g < b ? 6 : 0);
        else if (maxValue == g)
            h = (b - r) / d + 2;
        else if (maxValue == b)
            h = (r - g) / d + 4;
        h /= 6;
    }

    return float4(h, s, l, rgba.a);
}

float4 getPixelColor(float4 color, float2 texCoords)
{
    float4 texColor = !TexColBlend ? float4(0, 1, 0.5f, 1) : float4(0.5f, 0.5f, 0.5f, 1); //"Identity" HSL color, equivalent to modulating with RGBA = 1,1,1,1
    if (UseTexture)
    {
        texColor = tex2D(TextureSampler, texCoords);
        if (!TexColBlend)
            texColor = RgbaToHsla(texColor);
    }
    if (TexColBlend)
    {
        color = HslaToRgba(color) * texColor;
        color = saturate(color);
	}
    else
    {
        color.x += texColor.x;
        if (color.x >= 1)
            color.x -= (int) color.x;
        color.y *= texColor.y;
        color.z *= texColor.z;
        color = saturate(color);
        color = HslaToRgba(color);
    }
    return color;
}