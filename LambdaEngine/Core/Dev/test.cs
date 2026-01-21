namespace LambdaEngine.Core.Dev;

public class test {
    public class ChunkLayout {
        public const int CHUNK_SIZE = 16384;
        public const int HEADER_SIZE = 32;
        public const int ENTITY_ID_SIZE = 4;
        public const int ENTITY_ID_ALIGNMENT = 4;

        public static LayoutResult ComputeLayout(List<ComponentLayout> components) {
            int maxCapacity = BinarySearchCapacity(components);
            return ComputeOffsets(components, maxCapacity);
        }

        private static int BinarySearchCapacity(List<ComponentLayout> components) {
            int low = 1, high = 1024; // Conservative upper bound
            int best = 0;

            while (low <= high) {
                int mid = (low + high) / 2;
                int size = ComputeSize(components, mid);

                if (size <= CHUNK_SIZE) {
                    best = mid;
                    low = mid + 1;
                }
                else {
                    high = mid - 1;
                }
            }

            return best;
        }

        private static int ComputeSize(List<ComponentLayout> components, int N) {
            int offset = HEADER_SIZE;

            // Align for Entity IDs
            offset = AlignUp(offset, ENTITY_ID_ALIGNMENT);
            offset += ENTITY_ID_SIZE * N;
            
            foreach (ComponentLayout comp in components) {
                offset = AlignUp(offset, comp.Alignment);
                offset += comp.Size * N;
            }

            return offset;
        }

        private static LayoutResult ComputeOffsets(List<ComponentLayout> components, int N) {
            int offset = HEADER_SIZE;
            Dictionary<string, int> offsets = new();

            // Entity IDs
            offset = AlignUp(offset, ENTITY_ID_ALIGNMENT);
            offsets["EntityID"] = offset;
            offset += ENTITY_ID_SIZE * N;

            foreach (ComponentLayout comp in components) {
                offset = AlignUp(offset, comp.Alignment);
                offsets[comp.Name] = offset;
                offset += comp.Size * N;
            }

            return new LayoutResult {
                Capacity = N,
                Offsets = offsets,
                TotalSize = offset
            };
        }

        private static int AlignUp(int value, int alignment) {
            return (value + alignment - 1) & ~(alignment - 1);
        }

        public struct ComponentLayout {
            public int Size;
            public int Alignment;
            public string Name;
        }

        public struct LayoutResult {
            public int Capacity;
            public Dictionary<string, int> Offsets;
            public int TotalSize;
        }
    }
}