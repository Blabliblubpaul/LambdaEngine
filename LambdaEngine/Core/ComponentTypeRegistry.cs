using System.Runtime.InteropServices;

namespace LambdaEngine.Core;

internal static class ComponentTypeRegistry {
    public const ushort INVALID_COMPONENT = ushort.MaxValue;
    
    private static readonly Dictionary<Type, ushort> _typeToId = new();
    private static readonly Dictionary<ushort, Type> _idToType = new();
    private static ushort _nextId = 0;
    
    public static ushort Register<T>() where T : unmanaged, IEcsComponent{
        Type type = typeof(T);
        _typeToId[type] = _nextId;
        _idToType[_nextId] = type;
        
        return _nextId++;
    }

    public static int GetTypeSize(ushort id) {
        return Marshal.SizeOf(GetType(id));
    }

    public static Type GetType(ushort id) {
        if (_idToType.TryGetValue(id, out Type? type)) {
            return type;
        }
        
        throw new ArgumentException("Invalid component type.");
    }
    
    public static ushort GetId(Type type) {
        return _typeToId.GetValueOrDefault(type, INVALID_COMPONENT);
    }

    public static ushort GetId<T>() where T : unmanaged, IEcsComponent {
        return ComponentTypeId<T>.ID;
    }
}