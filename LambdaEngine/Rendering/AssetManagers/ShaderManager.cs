using LambdaEngine.Debug;
using SDL3;

namespace LambdaEngine.Rendering;

public class ShaderManager {
    public static readonly ShaderManager Instance = new();

    private IntPtr _gpuDevice;

    internal IntPtr _defaultTextureVertexShader, _defaultTextureFragmentShader;
    internal bool _hasDefaultTextureShaders;

    internal bool _hadInit;
    
    private ShaderManager() { }

    internal void Init(IntPtr gpuDevice) {
        _gpuDevice = gpuDevice;
    }

    public void LoadDefaultTextureShaders(string vertexShaderPath, string fragmentShaderPath) {
        if (_hadInit) {
            throw new Exception("Unable to load shaders; init phase is over");
        }
        
        _defaultTextureVertexShader = RenderingHelper.LoadShader(_gpuDevice, vertexShaderPath, 0, 1, 1, 0);
        _defaultTextureFragmentShader = RenderingHelper.LoadShader(_gpuDevice, fragmentShaderPath, 1, 0, 0, 0);

        if (_defaultTextureVertexShader == IntPtr.Zero || _defaultTextureFragmentShader == IntPtr.Zero) {
            throw new Exception("Default texture shaders not found.");
        }
        
        _hasDefaultTextureShaders = true;
        LDebug.Log("Default texture shaders loaded.");
    }

    internal void ReleaseShaders() {
        SDL.ReleaseGPUShader(_gpuDevice, _defaultTextureVertexShader);
        SDL.ReleaseGPUShader(_gpuDevice, _defaultTextureFragmentShader);
    }
}