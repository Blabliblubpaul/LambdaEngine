Entity:
- ChunkId
- Index

Chunk:
- Count
- Ptr

EntityManager:
- Entities

ArchetypeManager:
- Archetypes

Archetype:
- Composition
- Chunks

CreateEntity:
- > -> EntityManager.NextId()
- > -> ArchetypeManager.GetOrCreateArchetype()
- > -> Archetype.CreateEntity()
- > -> Chunks.Last.Set(endOfChunk, newEntity)

GetEntity:
- > EntityManager.Get()
- > Chunks.Get(chunkId, index)