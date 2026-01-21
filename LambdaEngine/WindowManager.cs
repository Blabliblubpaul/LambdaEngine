using LambdaEngine.Debug;
using LambdaEngine.Types;
using SDL3;

namespace LambdaEngine;

public static class WindowManager {
    private static LambdaEngine _engine;
    
    private static IntPtr _rendererHandle;
    private static IntPtr _windowHandle;

    public static int WindowWidth { get; private set; } = 800;

    public static int WindowHeight { get; private set; } = 600;
    
    public static ColorRgb BackgroundColor { get; set;} = ColorRgb.White;

    public static IntPtr WindowHandle {
        get => _windowHandle;
        set => _windowHandle = value;
    }

    public static IntPtr RendererHandle {
        get => _rendererHandle;
        set => _rendererHandle = value;
    }
    
    public static bool Initialize(LambdaEngine engine, string appName, string appVersion, string appIdentifier) {
        _engine = engine;
        
        SDL.SetAppMetadata(appName, appVersion, appIdentifier);

        if (!SDL.Init(SDL.InitFlags.Video)) {
            LDebug.Log($"Failed to initialize SDL: {SDL.GetError()}", LogLevel.FATAL);
            return false;
        }
        
        LDebug.Log("SDL3 initialized.");
        
        GameLoop.OnPollSdlEvents += HandleSdlEvents;
        
        return true;
    }

    public static void SetWindowSize(int width, int height) {
        WindowWidth = width;
        WindowHeight = height;
        
        SDL.SetWindowSize(_windowHandle, width, height);
    }

    internal static bool CreateWindow(string windowTitle) {
        _windowHandle = SDL.CreateWindow(
            windowTitle,
            WindowWidth,
            WindowHeight,
            SDL.WindowFlags.MouseFocus
        );

        if (_windowHandle == IntPtr.Zero) {
            LDebug.Log($"Failed to create window: {SDL.GetError()}", LogLevel.FATAL);
            return false;
        }

        _rendererHandle = SDL.CreateRenderer(_windowHandle, null);
        if (_rendererHandle == IntPtr.Zero) {
            LDebug.Log($"Failed to create renderer: {SDL.GetError()}", LogLevel.FATAL);
            SDL.DestroyWindow(_windowHandle);
            return false;
        }

        SDL.SetRenderVSync(_rendererHandle, 1);

        LDebug.Log("Window created.");
        
        SDL.ShowWindow(_windowHandle);

        return true;
    }

    // TODO: Add SDL event consumers with type, priority and result (CONSUME, PROPAGATE, etc)
    private static void HandleSdlEvents() {
        while (SDL.PollEvent(out SDL.Event @event)) {
            if (@event.Type == (uint)SDL.EventType.Quit) {
                _engine.Stop();
            }

            if (@event.Type == (uint)SDL.EventType.KeyDown) {
                Input.Instance.HandleSdlKeyDownEvent(@event);
            }
            
            if (@event.Type == (uint)SDL.EventType.KeyUp) {
                Input.Instance.HandleSdlKeyUpEvent(@event);
            }
        }
    }

    internal static void DestroyWindow() {
        SDL.DestroyRenderer(_rendererHandle);
        SDL.DestroyWindow(_windowHandle);
        
        SDL.Quit();
    }
}