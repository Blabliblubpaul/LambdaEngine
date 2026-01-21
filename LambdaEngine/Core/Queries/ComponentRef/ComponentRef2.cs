namespace LambdaEngine.Core.Queries.ComponentRef;

public readonly ref struct ComponentRef<T0, T1>
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent {
    public readonly int Id;
    
    private readonly ref T0 _item0;
    private readonly ref T1 _item1;

    internal ComponentRef(int id, ref T0 item0, ref T1 item1) {
        Id = id;
        
        _item0 = ref item0;
        _item1 = ref item1;
    }
    
    public ref T0 Item0 {
        get => ref _item0;
    }
    
    public ref T1 Item1 {
        get => ref _item1;
    }
}
