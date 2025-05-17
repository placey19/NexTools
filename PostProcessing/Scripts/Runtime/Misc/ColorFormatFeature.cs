using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorFormatFeature : ScriptableRendererFeature {

    public RenderPassEvent When = RenderPassEvent.BeforeRenderingPostProcessing;
    public GraphicsFormat ColorFormat = GraphicsFormat.B5G5R5A1_UNormPack16;
    public bool EnableInSceneView = true;

    private ColorFormatPass _pass;

    public override void Create() {
        _pass = new ColorFormatPass(ColorFormat) {
            renderPassEvent = When
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (renderingData.cameraData.isSceneViewCamera && !EnableInSceneView) {
            return;
        }

        renderer.EnqueuePass(_pass);
    }
}
