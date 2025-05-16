//UNITY_SHADER_NO_UPGRADE
#ifndef SOBEL_NODE_INCLUDED
#define SOBEL_NODE_INCLUDED

void Sobel_float(UnityTexture2D Texture, UnitySamplerState Sampler, float2 UV, out float3 Out)
{
    float x = 0;
    float y = 0;

    // Values are hardcoded for simplicity. Kernel values with zeroes are missed out for efficiency.
    x += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(-Texture.texelSize.x, -Texture.texelSize.y)).r * -1.0;
    x += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(-Texture.texelSize.x, 0)).r * -2.0;
    x += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(-Texture.texelSize.x, Texture.texelSize.y)).r * -1.0;

    x += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(Texture.texelSize.x, -Texture.texelSize.y)).r * 1.0;
    x += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(Texture.texelSize.x, 0)).r * 2.0;
    x += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(Texture.texelSize.x, Texture.texelSize.y)).r * 1.0;

    y += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(-Texture.texelSize.x, -Texture.texelSize.y)).r * -1.0;
    y += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(0, -Texture.texelSize.y)).r * -2.0;
    y += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(Texture.texelSize.x, -Texture.texelSize.y)).r * -1.0;

    y += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(-Texture.texelSize.x, Texture.texelSize.y)).r * 1.0;
    y += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(0, Texture.texelSize.y)).r * 2.0;
    y += SAMPLE_TEXTURE2D(Texture, Sampler, UV + float2(Texture.texelSize.x, Texture.texelSize.y)).r * 1.0;

    Out = sqrt(x * x + y * y);
}

#endif //SOBEL_NODE_INCLUDED
