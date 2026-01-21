namespace LambdaEngine.Core.Queries.ReadonlyComponentRef;

public readonly ref struct ReadonlyComponentRef<T0, T1>
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent {
    public readonly int Id;
    
    private readonly ref readonly T0 _item0;
    private readonly ref readonly T1 _item1;

    internal ReadonlyComponentRef(int id, ref readonly T0 item0, ref readonly T1 item1) {
        Id = id;
        
        _item0 = ref item0;
        _item1 = ref item1;
    }
    
    public ref readonly T0 Item0 {
        get => ref _item0;
    }
    
    public ref readonly T1 Item1 {
        get => ref _item1;
    }
}
