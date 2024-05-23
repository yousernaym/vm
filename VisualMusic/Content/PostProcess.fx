float saturationLevel = 1;

sampler2D TextureSampler = sampler_state
{
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	texture = <Texture>;
};

float4 PixelShaderFunction(float4 position : SV_Position, float4 color : COLOR0, float2 texCoords : TEXCOORD0) : COLOR0
{
	float4 texColor = tex2D(TextureSampler, texCoords);
	color *= texColor;
	float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));
	color.rgb = lerp(float3(gray, gray, gray), color.rgb, saturationLevel);
	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
	}
}
