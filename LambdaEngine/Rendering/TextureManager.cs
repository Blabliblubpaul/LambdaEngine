using LambdaEngine.Core;
using LambdaEngine.Debug;
using LambdaEngine.Interfaces;
using SDL3;

namespace LambdaEngine.Rendering;

public class TextureManager : IStagelessSystem {
    public static readonly TextureManager Instance = new();

    private HashSet<IntPtr> _textures = new();
    
    private IntPtr _renderer;
    
    private TextureManager() { }

    public Shader? LoadTextureFromFile(string path) {
        string shaderString = File.ReadAllText(path);
        
        IntPtr result = LoadShader(_renderer,  shaderString);

        if (result != IntPtr.Zero) {
            _textures.Add(result);
            
            return new Shader(result);
        }

        LDebug.Log(SDL.GetError(), LogLevel.ERROR);
        return null;
    }

    // TODO: look up sdl gpu bindings
    private static IntPtr LoadShader(IntPtr renderer, string shader) {
        throw new NotImplementedException();
    }

    public void DestroyTexture(Texture texture) {
        _textures.Remove(texture.Handle);
        SDL.DestroyTexture(texture.Handle);
    }
    
    public IntPtr Get(TextureId id) {
        throw new NotImplementedException();
    }

    public void OnSetup(LambdaEngine engine, EcsWorld world) { }
    
    public void OnStartup() {
        _renderer = WindowManager.GpuDeviceHandle;
        
        _textures = new HashSet<IntPtr>(128);
    }

    public void OnShutdown() {
        foreach (IntPtr texture in _textures) {
            SDL.DestroyTexture(texture);
        }
    }
}