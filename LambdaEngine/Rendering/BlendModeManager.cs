using LambdaEngine.Core;
using LambdaEngine.Debug;
using LambdaEngine.Interfaces;
using LambdaEngine.Rendering.Types;
using SDL3;

namespace LambdaEngine.Rendering;

public class BlendModeManager : IStagelessSystem {
    public static readonly BlendModeManager Instance = new();

    private HashSet<IntPtr> _blendModes = new();
    
    private IntPtr _renderer;
    
    private BlendModeManager() { }

    // TODO: Add blend mode
    public Texture? CreateBlendMode() {
        IntPtr result = LoadBlendMode(_renderer);

        if (result != IntPtr.Zero) {
            _blendModes.Add(result);
            
            return new Texture(result);
        }

        LDebug.Log(SDL.GetError(), LogLevel.ERROR);
        return null;
    }

    private IntPtr LoadBlendMode(IntPtr renderer) {
        throw new NotImplementedException();
    }

    public void OnSetup(LambdaEngine engine, EcsWorld world) { }
    
    public void OnStartup() {
        _renderer = WindowManager.GpuDeviceHandle;
        
        _blendModes = new HashSet<IntPtr>(128);
    }

    public void OnShutdown() {
        foreach (IntPtr texture in _blendModes) {
            SDL.DestroyTexture(texture);
        }
    }
}