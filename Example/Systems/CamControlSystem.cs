using System.Numerics;
using LambdaEngine;
using LambdaEngine.Core;
using LambdaEngine.Interfaces;
using LambdaEngine.Rendering;
using LambdaEngine.Types;

namespace Example.Systems;

public class CamControlSystem : ISystem {
    private const float camSpeed = 300f;
    
    public void OnSetup(LambdaEngine.LambdaEngine engine, EcsWorld world) { }
    
    public void OnStartup() { }
    
    public void OnExecute() {
        Vector2 direction = Vector2.Zero;

        if (Input.GetKeyDown(Keys.LEFT_ARROW)) {
            direction.X = -1;
        }
        else if (Input.GetKeyDown(Keys.RIGHT_ARROW)) {
            direction.X = 1;
        }
        
        if (Input.GetKeyDown(Keys.UP_ARROW)) {
            direction.Y = -1;
        }
        else if (Input.GetKeyDown(Keys.DOWN_ARROW)) {
            direction.Y = 1;
        }

        if (direction != Vector2.Zero) {
            direction = Vector2.Normalize(direction);
        }
        
        Camera.Position += direction * camSpeed * GameLoop.DeltaTime;
    }
    
    public void OnShutdown() { }
}