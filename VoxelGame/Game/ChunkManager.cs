using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

namespace VoxelGame.Game
{
    public class ChunkManager
    {
        private static readonly Dictionary<Vector2, Chunk> Chunks = new();
        private const int ChunkWidth = 16, ChunkHeight = 128;
        private const int RenderDistance = 2;

        public static void Initialize()
        {
            for (int x = -RenderDistance; x < RenderDistance; x++)
            {
                for (int z = -RenderDistance; z < RenderDistance; z++)
                {
                    Chunk chunk = new(x * ChunkWidth, z * ChunkWidth, ChunkWidth, ChunkHeight);
                    Chunks.Add(new(x * ChunkWidth, z * ChunkWidth), chunk);
                    chunk.Generate();
                }
            }
        }

        public static Dictionary<Vector2, Chunk> GetChunks()
        {
            return Chunks;
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
    }
}
