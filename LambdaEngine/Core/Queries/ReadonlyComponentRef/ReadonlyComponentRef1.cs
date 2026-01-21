namespace LambdaEngine.Core.Queries.ReadonlyComponentRef;

public readonly ref struct ReadonlyComponentRef<T0>
    where T0 : unmanaged, IEcsComponent {
    public readonly int Id;
    
    private readonly ref readonly T0 _item0;

    internal ReadonlyComponentRef(int id, ref readonly T0 item0) {
        Id = id;
        _item0 = ref item0;
    }
    
    public ref readonly T0 Item0 {
        get => ref _item0;
    }
}
