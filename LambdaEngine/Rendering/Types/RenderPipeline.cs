namespace LambdaEngine.Rendering;

public readonly struct RenderPipeline {
    public readonly Shader VertexShader;
    public readonly Shader FragmentShader;
    public readonly BlendMode BlendMode;
    
    public readonly RenderPipelineId RenderPipelineId;
    
    public RenderPipeline(Shader vertexShader, Shader fragmentShader, BlendMode blendMode, RenderPipelineId renderPipelineId) {
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
        BlendMode = blendMode;
        RenderPipelineId = renderPipelineId;
    }
}