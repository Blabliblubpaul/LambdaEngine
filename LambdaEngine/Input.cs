using System.Numerics;
using System.Runtime.CompilerServices;
using LambdaEngine.Core;
using LambdaEngine.Interfaces;
using LambdaEngine.Types;
using SDL3;

namespace LambdaEngine;

public unsafe class Input : ISystem {
    public static readonly Input Instance = new();

    private Input() { }

    private KeyState[] _keyStates = new KeyState[(int)SDL.Scancode.Count];

    private bool* _keyboardState;
    private int _keyNum;

    private ReadOnlySpan<bool> KeyboardState {
        get => new(_keyboardState, _keyNum);
    }

    public static Vector2 GetMousePosition() {
        return Instance.GetMousePositionImpl();
    }
    
    public Vector2 GetMousePositionImpl() {
        SDL.GetMouseState(out float x, out float y);
        
        return new Vector2(x, y);
    }

    /// <summary>
    /// Returns true if the specified key was pressed down in this frame.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool GetKeyPressed(Keys key) {
        return Instance.GetKeyPressedImpl(key);
    }

    private bool GetKeyPressedImpl(Keys key) {
        return _keyStates[(int)KeyToSdlScancode(key)] == KeyState.PRESSED;
    }

    /// <summary>
    /// Returns true if the specified key is currently held down.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool GetKeyDown(Keys key) {
        return Instance.GetKeyDownImpl(key);
    }

    private bool GetKeyDownImpl(Keys key) {
        return KeyboardState[(int)KeyToSdlScancode(key)];
    }

    /// <summary>
    /// Returns true if the specified key was released in this frame.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool GetKeyReleased(Keys key) {
        return Instance.GetKeyReleasedImpl(key);
    }

    private bool GetKeyReleasedImpl(Keys key) {
        return _keyStates[(int)KeyToSdlScancode(key)] == KeyState.RELEASED;
    }

    internal void HandleSdlKeyDownEvent(SDL.Event @event) {
        if (_keyStates[(int)@event.Key.Scancode] == KeyState.DOWN) {
            return;           
        }
        
        _keyStates[(int)@event.Key.Scancode] = KeyState.PRESSED;
    }

    internal void HandleSdlKeyUpEvent(SDL.Event @event) {
        _keyStates[(int)@event.Key.Scancode] = KeyState.RELEASED;
    }

    public void OnSetup(LambdaEngine engine, EcsWorld world) {
        fixed (bool* ptr = SDL.GetKeyboardState(out _keyNum)) {
            _keyboardState = ptr;
        }
    }

    public void OnStartup() { }

    public void OnExecute() {
        for (int i = 0; i < _keyStates.Length; i++) {
            if (_keyStates[i] == KeyState.PRESSED) {
                _keyStates[i] = KeyState.DOWN;
            }
            else if (_keyStates[i] == KeyState.RELEASED) {
                _keyStates[i] = KeyState.NONE;
            }
        }
    }

    public void OnShutdown() { }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SDL.Scancode KeyToSdlScancode(Keys key) {
        return (SDL.Scancode)key;
    }
}