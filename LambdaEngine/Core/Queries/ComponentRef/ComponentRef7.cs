namespace LambdaEngine.Core.Queries.ComponentRef;

public readonly ref struct ComponentRef<T0, T1, T2, T3, T4, T5, T6>
    where T0 : unmanaged, IEcsComponent
    where T1 : unmanaged, IEcsComponent
    where T2 : unmanaged, IEcsComponent
    where T3 : unmanaged, IEcsComponent
    where T4 : unmanaged, IEcsComponent
    where T5 : unmanaged, IEcsComponent
    where T6 : unmanaged, IEcsComponent {
    public readonly int Id;
    
    private readonly ref T0 _item0;
    private readonly ref T1 _item1;
    private readonly ref T2 _item2;
    private readonly ref T3 _item3;
    private readonly ref T4 _item4;
    private readonly ref T5 _item5;
    private readonly ref T6 _item6;

    internal ComponentRef(int id, ref T0 item0, ref T1 item1, ref T2 item2, ref T3 item3, ref T4 item4, ref T5 item5, ref T6 item6) {
        Id = id;
        
        _item0 = ref item0;
        _item1 = ref item1;
        _item2 = ref item2;
        _item3 = ref item3;
        _item4 = ref item4;
        _item5 = ref item5;
        _item6 = ref item6;
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

    public ref T3 Item3 {
        get => ref _item3;
    }

    public ref T4 Item4 {
        get => ref _item4;
    }

    public ref T5 Item5 {
        get => ref _item5;
    }

    public ref T6 Item6 {
        get => ref _item6;
    }
}