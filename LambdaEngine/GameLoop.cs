namespace LambdaEngine;

public static class GameLoop {
    #region LifecycleEvents

    internal static event Action? OnFrameStart;
    internal static event Action? OnPollSdlEvents;
    internal static event Action? OnEarlyUpdate;
    internal static event Action? OnFixedUpdate;
    internal static event Action? OnUpdate;
    internal static event Action? OnRender;
    internal static event Action? OnPreDestroy;
    internal static event Action? OnDestroy;
    internal static event Action? OnFrameEnd;
    #endregion

    public static bool Running { get; private set; }

    #region Fixed Time
    public static float FixedDeltaTime {
        get => (float)_fixedDeltaTime;
        set => _fixedDeltaTime = value;
    }
    
    public static double FixedDeltaTimeAsDouble {
        get => _fixedDeltaTime;
        set => _fixedDeltaTime = value;
    }

    public static float FixedRuntime {
        get => (float)_fixedTime;
    }

    public static double FixedRuntimeAsDouble {
        get => _fixedTime;
    }
    #endregion

    #region Time
    public static float DeltaTime {
        get => (float)(_deltaTime > _maximumDeltaTime ? _maximumDeltaTime : _deltaTime);
    }

    public static double DeltaTimeAsDouble {
        get => _deltaTime > _maximumDeltaTime ? _maximumDeltaTime : _deltaTime;
    }

    public static float Runtime {
        get => (float)_time;
    }

    public static double RuntimeAsDouble {
        get => _time;
    }

    public static float MaximumDeltaTime {
        get => (float)_maximumDeltaTime;
        set => _maximumDeltaTime = value;
    }
    
    public static double MaximumDeltaTimeAsDouble {
        get => _maximumDeltaTime;
        set => _maximumDeltaTime = value;
    }
    #endregion

    private static DateTime _start;
    
    private static double _previousTime;
    private static double _currentTime;
    
    private static double _fixedTimeAcc;
    private static double _fixedTime;
    private static double _fixedDeltaTime = 0.016666666666666666d;
    
    private static double _deltaTime;
    private static double _time;
    private static double _maximumDeltaTime = 0.3333333333333333d;

    public static void StartGameLoop() {
        if (Running) {
            throw new InvalidOperationException("Run() has already been called.");
        }
        
        _start = DateTime.Now;

        _fixedTimeAcc = 0;
        _time = 0;
        _fixedTime = 0;
        _previousTime = GetCurrentTime();

        Running = true;
        
        ExecuteGameLoop();
    }
    
    public static void StopGameLoop() {
        if (!Running) {
            return;
        }
        
        Running = false;
    }

    private static void ExecuteGameLoop() {
        while (Running) {
            _currentTime = GetCurrentTime();
            _deltaTime = _currentTime - _previousTime;

            _previousTime = _currentTime;
            
            _time += _deltaTime;
            _fixedTimeAcc += _deltaTime;
            
            OnFrameStart?.Invoke();
            
            OnPollSdlEvents?.Invoke();
            
            OnEarlyUpdate?.Invoke();
            

            while (_fixedTimeAcc > _fixedDeltaTime) {
                _fixedTimeAcc -= _fixedDeltaTime;
                _fixedTime += _fixedDeltaTime;
                
                OnFixedUpdate?.Invoke();
            }
            
            OnUpdate?.Invoke();
            
            OnRender?.Invoke();
            
            OnPreDestroy?.Invoke();
            OnDestroy?.Invoke();
            
            OnFrameEnd?.Invoke();
        }
    }

    private static double GetCurrentTime() {
        return (DateTime.Now - _start).TotalSeconds;
    }
}