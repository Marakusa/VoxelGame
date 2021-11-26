using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace VoxelGame.Game
{
    public class ChunkManager
    {
        private static readonly Dictionary<Vector2, Chunk> Chunks = new();
        private static Chunk GeneratingChunk;
        private const int ChunkWidth = 16, ChunkHeight = 128;
        private const int RenderDistance = 2;

        private static List<Vector2> _chunksToLoad = new();
        private static List<Vector2> _chunksToUnload = new();

        public static void Initialize()
        {
            Chunks.Clear();

            for (int x = -RenderDistance; x < RenderDistance; x++)
            {
                for (int y = -RenderDistance; y < RenderDistance; y++)
                {
                    AddChunkToLoad(x, y);
                    //Chunk chunk = new(x * ChunkWidth, z * ChunkWidth, ChunkWidth, ChunkHeight);
                    //GeneratedChunks.Add(new(x * ChunkWidth, z * ChunkWidth), chunk);
                    //chunk.Generate();
                    //Chunks.Add(new(x * ChunkWidth, z * ChunkWidth), chunk);
                }
            }
        }

        public static Dictionary<Vector2, Chunk> GetChunks()
        {
            return Chunks;
        }

        public static void LoadChunks(Vector3 position)
        {
            int chunkX = (int)(position.X / ChunkWidth);
            int chunkY = (int)(position.Y / ChunkWidth);

            for (int x = -RenderDistance + chunkX; x < RenderDistance + chunkX; x++)
            {
                for (int y = -RenderDistance + chunkY; y < RenderDistance + chunkY; y++)
                {
                    Vector2 c = new(x * ChunkWidth, y * ChunkWidth);

                    if (!_chunksToLoad.Contains(c))
                        AddChunkToLoad(x, y);
                }
            }
        }

        public static void AddChunkToLoad(int chunkX, int chunkY)
        {
            Vector2 c = new(chunkX * ChunkWidth, chunkY * ChunkWidth);

            if (!_chunksToLoad.Contains(c))
            {
                _chunksToLoad.Add(c);
                LoadChunks();
            }
        }

        public delegate void ChunkGeneratedHandler(Chunk chunk);
        public static event ChunkGeneratedHandler Generated;

        private static void LoadChunks()
        {
            if (GeneratingChunk == null && _chunksToLoad.Count > 0)
            {
                var loadChunk = _chunksToLoad[0];

                Chunk chunk = new((int)loadChunk.X, (int)loadChunk.Y, ChunkWidth, ChunkHeight);
                GeneratingChunk = chunk;
                chunk.Generated += (sender, vertices, indices) =>
                {
                    if (chunk.IsGenerated)
                    {
                        if (!Chunks.ContainsKey(loadChunk))
                            Chunks.Add(loadChunk, GeneratingChunk);
                        _chunksToLoad.Remove(loadChunk);
                        GeneratingChunk = null;
                        Generated?.Invoke(Chunks[loadChunk]);
                        LoadChunks();
                    }
                };

                ThreadStart threadStart = new ThreadStart(chunk.Generate);
                Thread thread = new(threadStart);
                thread.Start();
            }
        }

        public static Chunk GetChunkByPoint(Vector3 point)
        {
            Vector2 flooredPoint = new((float)Math.Floor(point.X / ChunkWidth) * ChunkWidth, (float)Math.Floor(point.Z / ChunkWidth) * ChunkWidth);
            
            if (Chunks.ContainsKey(flooredPoint))
                return Chunks[flooredPoint];

            return null;
        }

        public static void DeleteChunkBuffers()
        {
            foreach (var chunk in Chunks)
            {
                chunk.Value.DeleteBuffers();
            }
        }

        public static float GetWidth()
        {
            return ChunkWidth;
        }
        public static float GetHeight()
        {
            return ChunkHeight;
        }
    }
}
