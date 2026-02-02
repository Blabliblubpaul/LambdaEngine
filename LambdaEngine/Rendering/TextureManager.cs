using LambdaEngine.Core;
using LambdaEngine.Debug;
using LambdaEngine.Interfaces;
using LambdaEngine.Rendering.Types;
using SDL3;

namespace LambdaEngine.Rendering;

public unsafe class TextureManager : IStagelessSystem {
    private const int MAX_TEXTURES = 0xFFFFFF;
    
    public static readonly TextureManager Instance = new();

    internal List<IntPtr> _textures = new(256);

    internal bool hadInit;
    
    private TextureManager() { }

    /// <summary>
    /// Registers a texture to be loaded.
    /// <remarks>Since all textures are loaded by the render system on startup,
    /// textures may only be registered before.</remarks>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="channels"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Texture RegisterTexture(string path, int channels = 4) {
        if (hadInit) {
            throw new Exception("Unable to register textures; texture init is already done.");
        }

        if (_textures.Count >= MAX_TEXTURES) {
            throw new Exception("Texture count is larger than the maximum number of textures.");
        }
        
        SDL.Surface* texture = RenderingHelper.LoadImage(path, channels);

        _textures.Add(new IntPtr(texture));
        int id = _textures.Count;
        
        return new Texture(texture, id);
    }
    
    public IntPtr Get(TextureId id) {
        throw new NotImplementedException();
    }

    public void OnSetup(LambdaEngine engine, EcsWorld world) { }
    
    public void OnStartup() { }

    public void OnShutdown() { }
}