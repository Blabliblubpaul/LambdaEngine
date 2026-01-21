namespace LambdaEngine.Core.Queries.ComponentRef;

public readonly ref struct ComponentRef<T0, T1, T2>
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent
    where T2 : unmanaged, IEcsComponent {
    public readonly int Id;
    
    private readonly ref T0 _item0;
    private readonly ref T1 _item1;
    private readonly ref T2 _item2;
    
    internal ComponentRef(int id, ref T0 item0, ref T1 item1, ref T2 item2) {
        Id = id;
        
        _item2 = ref item2;
        _item1 = ref item1;
        _item0 = ref item0;
    }


    public ref T0 Item0 {
        get => ref _item0;
    }

    public ref T1 Item1 {
        get => ref _item1;
    }
    
    public ref T2 Item2 {
        get => ref _item2;
    }
}