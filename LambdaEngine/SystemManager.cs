using LambdaEngine.Core;
using LambdaEngine.Interfaces;
using LambdaEngine.Types;

// ReSharper disable MemberHidesStaticFromOuterClass

namespace LambdaEngine;

// TODO: Add bootstrap stage for window/gpuDevice creation etc

// TODO: Before/After priorities.
// TODO: System Base instead of independent interfaces.
// TODO: Freeze configuration after Configure().
// TODO: Add more specific init/loaded stages
// TODO: Add system lifetimes/destruction
public static class SystemManager {
    private static LambdaEngine _engine;

    private static IStagelessSystem[] _stagelessSystems;
    
    private static ISystem[] _frameStartSystems;
    private static ISystem[] _earlyUpdateSystems;
    private static ISystem[] _fixedUpdateSystems;
    private static ISystem[] _updateSystems;
    private static ISystem[] _renderSystems;
    private static ISystem[] _destroySystems;

    private static bool _configured;
    private static bool _initialized;
    
    internal static void Initialize(LambdaEngine engine) {
        if (!_configured) {
            throw new InvalidOperationException("SystemManager has not been configured.");
        }
        
        _engine = engine;
        
        GameLoop.OnFrameStart += () => ExecuteStage(SystemStage.FRAME_START);
        GameLoop.OnEarlyUpdate += () => ExecuteStage(SystemStage.EARLY_UPDATE);
        GameLoop.OnFixedUpdate += () => ExecuteStage(SystemStage.FIXED_UPDATE);
        GameLoop.OnUpdate += () => ExecuteStage(SystemStage.UPDATE);
        GameLoop.OnRender += () => ExecuteStage(SystemStage.RENDER);
        GameLoop.OnDestroy += () => ExecuteStage(SystemStage.ENTITY_DESTRUCTION);
        
        _initialized = true;
    }

    internal static void SystemSetup(EcsWorld world) {
        if (!_initialized) {
            throw new InvalidOperationException("SystemManager has not been configured.");
        }

        foreach (IStagelessSystem system in _stagelessSystems) {
            system.OnSetup(_engine, world);
        }
        
        foreach (ISystem system in _frameStartSystems) {
            system.OnSetup(_engine, world);       
        }

        foreach (ISystem system in _earlyUpdateSystems) {
            system.OnSetup(_engine, world);       
        }

        foreach (ISystem system in _fixedUpdateSystems) {
            system.OnSetup(_engine, world);
        }

        foreach (ISystem system in _updateSystems) {
            system.OnSetup(_engine, world);
        }

        foreach (ISystem system in _renderSystems) {
            system.OnSetup(_engine, world);
        }
        
        foreach (ISystem system in _destroySystems) {
            system.OnSetup(_engine, world);
        }
    }

    internal static void SystemStartup() {
        if (!_initialized) {
            throw new InvalidOperationException("SystemManager has not been configured.");
        }

        foreach (IStagelessSystem system in _stagelessSystems) {
            system.OnStartup();       
        }
        
        foreach (ISystem system in _frameStartSystems) {
            system.OnStartup();      
        }

        foreach (ISystem system in _earlyUpdateSystems) {
            system.OnStartup();      
        }

        foreach (ISystem system in _fixedUpdateSystems) {
            system.OnStartup();
        }

        foreach (ISystem system in _updateSystems) {
            system.OnStartup();
        }

        foreach (ISystem system in _renderSystems) {
            system.OnStartup();
        }
        
        foreach (ISystem system in _destroySystems) {
            system.OnStartup();
        }
    }

    internal static void SystemShutdown() {
        if (!_initialized) {
            throw new InvalidOperationException("SystemManager has not been configured.");
        }

        foreach (IStagelessSystem system in _stagelessSystems) {
            system.OnShutdown();      
        }
        
        foreach (ISystem system in _frameStartSystems) {
            system.OnShutdown();     
        }

        foreach (ISystem system in _earlyUpdateSystems) {
            system.OnShutdown();     
        }

        foreach (ISystem system in _fixedUpdateSystems) {
            system.OnShutdown();
        }

        foreach (ISystem system in _updateSystems) {
            system.OnShutdown();
        }

        foreach (ISystem system in _renderSystems) {
            system.OnShutdown();
        }
        
        foreach (ISystem system in _destroySystems) {
            system.OnShutdown();
        }
    }

    private static void ExecuteStage(SystemStage stage) {
        if (!_initialized) {
            throw new InvalidOperationException("SystemManager has not been configured.");
        }

        ISystem[] systems = stage switch {
            SystemStage.FRAME_START => _frameStartSystems,
            SystemStage.EARLY_UPDATE => _earlyUpdateSystems,
            SystemStage.FIXED_UPDATE => _fixedUpdateSystems,
            SystemStage.UPDATE => _updateSystems,
            SystemStage.RENDER => _renderSystems,
            SystemStage.ENTITY_DESTRUCTION => _destroySystems,
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
        };

        foreach (ISystem system in systems) {
            system.OnExecute();
        }
    }

    public sealed class SystemManagerConfigurator {
        private readonly List<(int priority, IStagelessSystem system)> _stagelessSystems = new(8);
        
        private readonly List<(int priority, ISystem system)> _frameStartSystems = new(8);
        private readonly List<(int priority, ISystem system)> _earlyUpdateSystems = new(8);
        private readonly List<(int priority, ISystem system)> _fixedUpdateSystems = new(8);
        private readonly List<(int priority, ISystem system)> _updateSystems = new(8);
        private readonly List<(int priority, ISystem system)> _renderSystems = new(8);
        private readonly List<(int priority, ISystem system)> _destroySystems = new(8);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="priority">Smaller value -> higher priority</param>
        /// <typeparam name="TSystem"></typeparam>
        /// <returns></returns>
        public SystemManagerConfigurator RegisterSystem<TSystem>(SystemStage stage, int priority)
            where TSystem : ISystem, new() {
            return RegisterSystem(new TSystem(), stage, priority);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="priority">Smaller value -> higher priority</param>
        /// <typeparam name="TSystem"></typeparam>
        /// <returns></returns>
        public SystemManagerConfigurator RegisterStagelessSystem<TSystem>(int priority)
            where TSystem : IStagelessSystem, new() {
            return RegisterStagelessSystem(new TSystem(), priority);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="priority">Smaller value -> higher priority</param>
        /// <typeparam name="TSystem"></typeparam>
        /// <returns></returns>
        public SystemManagerConfigurator RegisterStagelessSystem(IStagelessSystem system, int priority) {
            _stagelessSystems.Add(new ValueTuple<int, IStagelessSystem>(priority, system));

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="priority">Smaller value -> higher priority</param>
        /// <returns></returns>
        public SystemManagerConfigurator RegisterSystem(ISystem system, SystemStage stage, int priority) {
            switch (stage) {
                case SystemStage.FRAME_START:
                    _frameStartSystems.Add(new ValueTuple<int, ISystem>(priority, system));
                    break;
                
                case SystemStage.EARLY_UPDATE:
                    _earlyUpdateSystems.Add(new ValueTuple<int, ISystem>(priority, system));
                    break;
                
                case SystemStage.FIXED_UPDATE:
                    _fixedUpdateSystems.Add(new ValueTuple<int, ISystem>(priority, system));
                    break;

                case SystemStage.UPDATE:
                    _updateSystems.Add(new ValueTuple<int, ISystem>(priority, system));
                    break;

                case SystemStage.RENDER:
                    _renderSystems.Add(new ValueTuple<int, ISystem>(priority, system));
                    break;
                
                case SystemStage.ENTITY_DESTRUCTION:
                    _destroySystems.Add(new ValueTuple<int, ISystem>(priority, system));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }

            return this;
        }

        public void Configure() {
            if (_initialized) {
                throw new InvalidOperationException("SystemManager has already been configured.");
            }

            _stagelessSystems.Sort((x, y) => x.priority - y.priority);
            
            _frameStartSystems.Sort((x, y) => x.priority - y.priority);
            _earlyUpdateSystems.Sort((x, y) => x.priority - y.priority);
            _fixedUpdateSystems.Sort((x, y) => x.priority - y.priority);
            _updateSystems.Sort((x, y) => x.priority - y.priority);
            _renderSystems.Sort((x, y) => x.priority - y.priority);
            _destroySystems.Sort((x, y) => x.priority - y.priority);

            SystemManager._stagelessSystems = _stagelessSystems.Select(o => o.system).ToArray();           
            
            SystemManager._frameStartSystems = _frameStartSystems.Select(o => o.system).ToArray();           
            SystemManager._earlyUpdateSystems = _earlyUpdateSystems.Select(o => o.system).ToArray();           
            SystemManager._fixedUpdateSystems = _fixedUpdateSystems.Select(o => o.system).ToArray();
            SystemManager._updateSystems = _updateSystems.Select(o => o.system).ToArray();
            SystemManager._renderSystems = _renderSystems.Select(o => o.system).ToArray();
            SystemManager._destroySystems = _destroySystems.Select(o => o.system).ToArray();

            _configured = true;
        }
    }
}