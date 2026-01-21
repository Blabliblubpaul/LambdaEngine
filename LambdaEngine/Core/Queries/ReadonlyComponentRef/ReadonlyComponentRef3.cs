namespace LambdaEngine.Core.Queries.ReadonlyComponentRef;

public readonly ref struct ReadonlyComponentRef<T0, T1, T2>
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent
    where T2 : unmanaged, IEcsComponent {
    public readonly int Id;
    
    private readonly ref readonly T0 _item0;
    private readonly ref readonly T1 _item1;
    private readonly ref readonly T2 _item2;
    
    internal ReadonlyComponentRef(int id, ref readonly T0 item0, ref readonly T1 item1, ref readonly T2 item2) {
        Id = id;
        
        _item2 = ref item2;
        _item1 = ref item1;
        _item0 = ref item0;
    }


    public ref readonly T0 Item0 {
        get => ref _item0;
    }

    public ref readonly T1 Item1 {
        get => ref _item1;
    }
    
    public ref readonly T2 Item2 {
        get => ref _item2;
    }
}