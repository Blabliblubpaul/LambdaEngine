namespace LambdaEngine.Core.Queries;

// public unsafe class Simd128ComponentEnumerator<T> : IEnumerator<Vector128<T>> where T : unmanaged, EcsComponent {
//     private const int TSize8 = 8;
//     private const int TSize16 = 16;
//     private const int TSize32 = 32;
//     private const int TSize64 = 64;
//     
//     private readonly Memory<T>[] _components;
//
//     private readonly int _tSize;
//     private readonly int _tCount;
//     private bool _isAtEnd;
//     private bool _hadFirstMove;
//     
//     public PrimitiveTypes InterpretAs { get; set; }
//
//     public Vector128<T> Current {
//         get => _isAtEnd && _hadFirstMove
//             ? throw new InvalidOperationException()
//             : _components[_arrayIndex].Span[_componentIndex];
//     }
//
//     public int CurrentCount {
//         get => 0;
//     }
//
//     object IEnumerator.Current {
//         get => Current;
//     }
//
//     private int _arrayIndex = 0;
//     private int _componentIndex = -1;
//     
//     internal Simd128ComponentEnumerator(Memory<T>[] components) {
//         _tSize = sizeof(T) switch {
//             TSize8 => TSize8,
//             TSize16 => TSize16,
//             TSize32 => TSize32,
//             TSize64 => TSize64,
//             _ => throw new ArgumentException("Invalid component size for 128 SIMD enumerator.")
//         };
//         
//         _tCount = 8 / _tSize;
//         
//         _components = components;
//     }
//     
//     public bool MoveNext() {
//         _componentIndex++;
//         
//         if (_components[_arrayIndex].Length == _componentIndex) {
//             _arrayIndex++;
//             
//             if (_components.Length == _arrayIndex) {
//                 _isAtEnd = true;
//                 return false;
//             }
//
//             _componentIndex = 0;
//         }
//         
//         return true;
//     }
//
//     private Vector128<T> GetCurrentVector() {
//         T[] values = new T[_tCount];
//
//         int arrayIndex = _arrayIndex;
//         int componentIndex = _componentIndex;
//
//         bool end = false;
//         for (int i = 0; i < _tCount; i++) {
//             while (_components[arrayIndex].Length == componentIndex) {
//                 arrayIndex++;
//                 
//                 if (_components.Length == arrayIndex) {
//                     end = true;
//                     break;
//                 }
//                 
//                 componentIndex = 0;
//             }
//
//             if (end) {
//                 break;
//             }
//             
//             values[i] = _components[arrayIndex].Span[componentIndex];
//             componentIndex++;
//         }
//         
//         return Vector128.Create();
//     }
//     
//     public void Reset() {
//         _arrayIndex = 0;
//         _componentIndex = -1;
//
//         _isAtEnd = false;
//     }
//
//     public void Dispose() {
//         GC.SuppressFinalize(this);
//     }
// }