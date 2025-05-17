using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Nexcide.PostProcessing {

    public class PostProcessPass : ScriptableRenderPass {

        private readonly VolumeEffect _effect;
        private RTHandle _colorCopy;        // The handle to the temporary color copy texture (only used in the non-render graph path)

        private static readonly int _blitTexture = Shader.PropertyToID("_BlitTexture");
        private static readonly int _blitScaleBias = Shader.PropertyToID("_BlitScaleBias");

        public PostProcessPass(VolumeEffect effect) {
            _effect = effect;

            profilingSampler = new ProfilingSampler(_effect.ShaderName);
            requiresIntermediateTexture = true;     // intend to sample the active color buffer
        }

        public bool IsEffectActive() {
            return _effect.ConfigureMaterial(VolumeManager.instance?.stack, out _);
        }

        // This method contains the shared rendering logic for doing the temporary color copy pass (used by both the non-render graph and render graph paths)
        private void ExecuteCopyColorPass(RasterCommandBuffer cmd, RTHandle sourceTexture) {
            Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
        }

        // This method is used to get the descriptor used for creating the temporary color copy texture that will enable the main pass to sample the screen color
        private RenderTextureDescriptor GetCopyPassTextureDescriptor(RenderTextureDescriptor desc) {
            // Unless 'desc.bindMS = true' for an MSAA texture a resolve pass will be inserted before it is bound for sampling.
            // Since our main pass shader does not expect to sample an MSAA target we will leave 'bindMS = false'.
            // If the camera target has MSAA enabled an MSAA resolve will still happen before our copy-color pass but
            // with this change we will avoid an unnecessary MSAA resolve before our main pass.
            desc.msaaSamples = 1;

            // This avoids copying the depth buffer tied to the current descriptor as the main pass in this example does not use it
            desc.depthBufferBits = (int)DepthBits.None;

            return desc;
        }

        [System.Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            // This ScriptableRenderPass manages its own RenderTarget.
            // ResetTarget here so that ScriptableRenderer's active attachment can be invalidated when processing this ScriptableRenderPass.
            ResetTarget();

            // This allocates our intermediate texture and makes sure it's reallocated if some settings on the camera target change (e.g. resolution)
            RenderingUtils.ReAllocateHandleIfNeeded(ref _colorCopy, GetCopyPassTextureDescriptor(renderingData.cameraData.cameraTargetDescriptor), name: "_CustomPostPassCopyColor");
        }

        [System.Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled). Use Render Graph API instead.", false)]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get();
            cmd.Clear();

            using (new ProfilingScope(cmd, profilingSampler)) {
                RasterCommandBuffer rasterCmd = CommandBufferHelpers.GetRasterCommandBuffer(cmd);
                ref CameraData cameraData = ref renderingData.cameraData;

                CoreUtils.SetRenderTarget(cmd, _colorCopy);
                ExecuteCopyColorPass(rasterCmd, cameraData.renderer.cameraColorTargetHandle);

                CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle);
                ExecuteMainPass(rasterCmd, sourceTexture: _colorCopy);
            }

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }

        // This method contains the shared rendering logic for doing the main post-processing pass (used by both the non-render graph and render graph paths)
        private void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle sourceTexture) {
            VolumeStack volumeStack = VolumeManager.instance?.stack;

            if (volumeStack != null && _effect.ConfigureMaterial(volumeStack, out Material material)) {
                if (sourceTexture != null) {
                    material.SetTexture(_blitTexture, sourceTexture);
                }

                // This uniform needs to be set for user materials with shaders relying on core Blit.hlsl to work as expected
                material.SetVector(_blitScaleBias, new Vector4(1, 1, 0, 0));

                cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3, 1);
            }
        }

        private class MainPassData {
            public TextureHandle inputTexture;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // Below is an example of a typical post-processing effect which samples from the current color 
            // Feel free modify/rename/add additional or remove the existing passes based on the needs of your custom post-processing effect
            using (var builder = renderGraph.AddRasterRenderPass<MainPassData>(passName, out var passData, profilingSampler)) {
                // GPU graphics pipelines don't allow to sample the texture bound as the active color target, ie the cameraColor cannot both be an input and the render target.
                // Before, this required us to first copy the cameraColor to then blit back to it while sampling from the copy. Now that we have the ContextContainer, we can swap
                // the cameraColor to another (temp) resource so that the next pass uses the temp resource. We don't need the copy anymore. However, this only works if you are
                // writing to every pixel of the frame, a partial write will need the copy first to add to the existing content. See FullScreenPassRendererFeature.cs for an example. 
                TextureDesc cameraColorDesc = renderGraph.GetTextureDesc(resourcesData.cameraColor);
                cameraColorDesc.name = "_CameraColorCustomPostProcessing";
                cameraColorDesc.clearBuffer = false;

                TextureHandle destination = renderGraph.CreateTexture(cameraColorDesc);
                passData.inputTexture = resourcesData.cameraColor;

                // If you use framebuffer fetch in your material then you need to use builder.SetInputAttachment. If the pass can be merged then this will reduce GPU bandwidth
                // usage / power consumption and improve GPU performance. 
                builder.UseTexture(passData.inputTexture, AccessFlags.Read);

                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc((MainPassData data, RasterGraphContext context) => {
                    ExecuteMainPass(context.cmd, data.inputTexture.IsValid() ? data.inputTexture : null);
                });

                // Swap cameraColor to the new temp resource (destination) for the next pass
                resourcesData.cameraColor = destination;
            }
        }

        public void Dispose() {
            _colorCopy?.Release();
        }
    }
}
