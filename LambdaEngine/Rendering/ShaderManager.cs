using LambdaEngine.Core;
using LambdaEngine.Debug;
using LambdaEngine.Interfaces;
using SDL3;

namespace LambdaEngine.Rendering;

public class ShaderManager : IStagelessSystem {
    public static readonly ShaderManager Instance = new();

    private HashSet<IntPtr> _shaders = new();
    
    private IntPtr _renderer;
    
    private ShaderManager() { }

    public Shader? LoadShaderFromFile(string path) {
        IntPtr result = Image.LoadTexture(_renderer, path);

        if (result != IntPtr.Zero) {
            _shaders.Add(result);
            
            return new Texture(result);
        }

        LDebug.Log(SDL.GetError(), LogLevel.ERROR);
        return null;
    }

    public void OnSetup(LambdaEngine engine, EcsWorld world) { }
    
    public void OnStartup() {
        _renderer = WindowManager.GpuDeviceHandle;
        
        _shaders = new HashSet<IntPtr>(128);
    }

    public void OnShutdown() {
        foreach (IntPtr texture in _shaders) {
            SDL.DestroyTexture(texture);
        }
    }
}