using LambdaEngine.Core;
using LambdaEngine.Interfaces;
using LambdaEngine.Rendering;

namespace Example.Systems;

public class TextureSystem : IStagelessSystem {
    public static TextureSystem Instance = new();
    
    public Texture PlayerTexture { get; private set; }
    
    private TextureSystem() { }

    public void OnSetup(LambdaEngine.LambdaEngine engine, EcsWorld world) {
    }
    
    public void OnStartup() {
        PlayerTexture = TextureManager.Instance.LoadTextureFromFile("Assets/player.png")!;
    }
    
    public void OnShutdown() { }
}