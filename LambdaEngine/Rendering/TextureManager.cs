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

    public Texture? LoadTextureFromFile(string path) {
        IntPtr result = Image.LoadTexture(_renderer, path);

        if (result != IntPtr.Zero) {
            _textures.Add(result);
            
            return new Texture(result);
        }

        LDebug.Log(SDL.GetError(), LogLevel.ERROR);
        return null;
    }

    public void DestroyTexture(Texture texture) {
        _textures.Remove(texture.Handle);
        SDL.DestroyTexture(texture.Handle);
    }

    public void OnSetup(LambdaEngine engine, EcsWorld world) { }
    
    public void OnStartup() {
        _renderer = WindowManager.RendererHandle;
        
        _textures = new HashSet<IntPtr>(128);
    }

    public void OnShutdown() {
        foreach (IntPtr texture in _textures) {
            SDL.DestroyTexture(texture);
        }
    }
}