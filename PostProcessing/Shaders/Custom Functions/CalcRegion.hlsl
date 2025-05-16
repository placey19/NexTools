//UNITY_SHADER_NO_UPGRADE
#ifndef CALC_REGION_INCLUDED
#define CALC_REGION_INCLUDED

// Given a region bound and a centre-pixel UV, calculate the mean and variance of the region.
void CalcRegion_float(UnityTexture2D Texture, float2 UV, float2 Lower, float2 Upper, float Samples, out float3 Mean, out float Variance)
{
	float3 sum = 0.0;
	float3 squareSum = 0.0;

	for (int x = Lower.x; x <= Upper.x; ++x)
	{
		for (int y = Lower.y; y <= Upper.y; ++y)
		{
			float2 offset = float2(Texture.texelSize.x * x, Texture.texelSize.y * y);
			float3 color = SAMPLE_TEXTURE2D(Texture, Texture.samplerstate, UV + offset).rgb;
			sum += color;
			squareSum += color * color;
		}
	}

	Samples = trunc(Samples);
	Mean = sum / Samples;
	Variance = abs((squareSum / Samples) - (Mean * Mean)).r;
	Variance = length(Variance);
}

#endif  //CALC_REGION_INCLUDED
