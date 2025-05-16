Shader "Nexcide/Barebones"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        ZTest Never ZWrite Off Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                return dot(col.rgb, float3(0.2126729, 0.7151522, 0.072175));
            }

            ENDHLSL
        }
    }
}
