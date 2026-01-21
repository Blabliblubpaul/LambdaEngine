namespace LambdaEngine.Core;

internal static class ComponentTypeId<T> where T : unmanaged, IEcsComponent {
    public static readonly ushort ID;

    static ComponentTypeId() {
        ID = ComponentTypeRegistry.Register<T>();
    }
}