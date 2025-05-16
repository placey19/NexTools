//UNITY_SHADER_NO_UPGRADE
#ifndef COLORBLEEDNODE_INCLUDED
#define COLORBLEEDNODE_INCLUDED

void ColorBleed_float(UnityTexture2D Texture, UnitySamplerState Sampler, float2 UV, float Iterations, float MinAmount, float MaxAmount, float ChromaticAberration, out float3 Out)
{
    // make sure we have an integer number of iterations of at least 1
    Iterations = max(trunc(Iterations), 1.0);

    // adjust the amount over time
    float amount = lerp(MinAmount, MaxAmount, sin(_Time.y) * 0.5 + 0.5);

    // adjust the X of the UV coordinates to keep the texture centered during the iterative sampling
    UV.x += (Iterations + 1.0) / 2.0 * -amount * Texture.texelSize.x;

    // sample the texture multiple times at different offsets
    float offset = 0;
    for (int i = 0; i < Iterations; i++) {
        offset += amount;
        Out.r += Texture.Sample(Sampler, UV + float2(offset + ChromaticAberration, 0) * Texture.texelSize.xy).r;
        Out.g += Texture.Sample(Sampler, UV + float2(offset - ChromaticAberration, 0) * Texture.texelSize.xy).g;
        Out.b += Texture.Sample(Sampler, UV + float2(offset,                       0) * Texture.texelSize.xy).b;
    }

    Out /= Iterations;
}

#endif  //COLORBLEEDNODE_INCLUDED
