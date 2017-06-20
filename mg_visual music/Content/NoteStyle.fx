float4x4 WvpMat;
float2 ViewportSize;
float2 TexSize;
texture Texture;

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
