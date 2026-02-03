using LambdaEngine.Core;
using LambdaEngine.Debug;
using LambdaEngine.Interfaces;
using SDL3;

namespace LambdaEngine.Rendering;

public class RenderPipelineManager : IStagelessSystem {
    private const uint MAX_PIPELINES = RenderPipelineId.MaxValue;

    public static readonly RenderPipelineManager Instance = new();

    // TODO: Use unmanaged memory
    private List<RenderPipeline> _pipelines;
    
    // TODO: Clean this up
    public string DefaultVertexShaderPath;
    public string DefaultFragmentShaderPath;
    public IntPtr DefaultPipeline;
    
    private uint _nextId;

    private RenderPipelineManager() {
        _pipelines = new List<RenderPipeline>(128);
    }

    public RenderPipelineId CreatePipeline(Shader vertexShader, Shader fragmentShader, BlendMode blendMode) {
        if (_nextId >= MAX_PIPELINES) {
            throw new InvalidOperationException("Unable to create pipeline; maximum amount of pipelines already created.");
        }

        RenderPipelineId id = new(_nextId++);
        RenderPipeline pipeline = new(vertexShader, fragmentShader,  blendMode, id);

        _pipelines.Add(pipeline);
        
        return id;
    }

    public IntPtr Get(RenderPipelineId id) {
        throw new NotImplementedException();
    }
    
    public void OnSetup(LambdaEngine engine, EcsWorld world) { }
    
    public void OnStartup() { }

    public void OnShutdown() { }
}