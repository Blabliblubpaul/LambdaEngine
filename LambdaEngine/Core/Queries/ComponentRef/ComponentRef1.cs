namespace LambdaEngine.Core.Queries.ComponentRef;

public readonly ref struct ComponentRef<T0>
    where T0 : unmanaged, IEcsComponent {
    public readonly int Id;
    
    private readonly ref T0 _item0;

    internal ComponentRef(int id, ref T0 item0) {
        Id = id;
        _item0 = ref item0;
    }
    
    public ref T0 Item0 {
        get => ref _item0;
    }
}
