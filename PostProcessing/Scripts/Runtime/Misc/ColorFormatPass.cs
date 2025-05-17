using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

class ColorFormatPass : ScriptableRenderPass {

    private readonly GraphicsFormat _colorFormat;

    public ColorFormatPass(GraphicsFormat colorFormat) {
        _colorFormat = colorFormat;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();

        TextureDesc destinationDesc = renderGraph.GetTextureDesc(resourcesData.cameraColor);
        destinationDesc.name = "_CameraColorCopy";
        destinationDesc.clearBuffer = false;
        destinationDesc.colorFormat = _colorFormat;

        TextureHandle source = resourcesData.cameraColor;
        TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

        renderGraph.AddBlitPass(source, destination, scale: Vector2.one, offset: Vector2.zero);

        // set camera target to new texture
        resourcesData.cameraColor = destination;
    }
}
