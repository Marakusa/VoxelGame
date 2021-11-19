using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace VoxelGame.Game
{
    public class Chunk
    {
        private const int Width = 2, Height = 1;

        private Block[,,] _blocks;

        private List<float> _vertices = new();
        private List<int> _indices = new();

        public delegate void GeneratedHandler(object sender, float[] vertices, int[] indices);

        public event GeneratedHandler Generated;

        public Chunk()
        {
            _blocks = new Block[Width, Height, Width];
        }

        public void Generate()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Width; z++)
                    {
                        _blocks[x, y, z] = Blocks.Get("dirt");
                    }
                }
            }

            GenerateMesh();
        }

        private int _indicesIndex = 0;

        private void GenerateMesh()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Width; z++)
                    {
                        //if (!HasBlock(x, y, z + 1))
                        GenerateMeshFront(x, y, z);
                        //if (!HasBlock(x, y, z - 1))
                        GenerateMeshBack(x, y, z);
                    }
                }
            }

            Generated?.Invoke(this, _vertices.ToArray(), _indices.ToArray());
        }

        private bool HasBlock(int x, int y, int z)
        {
            if (x >= 0 && y >= 0 && z >= 0 && x < Width && y < Height && z < Width)
                return _blocks[x, y, z] != null;
            else
                return false;
        }
        
        private void GenerateMeshFront(int x, int y, int z)
        {
            UVTransform transform = _blocks[x, y, z].Texture.FrontTexture;

            float ux = transform.UvX;
            float uy = transform.UvY;
            float uw = transform.UvW;
            float uh = transform.UvH;

            _vertices.AddRange(new[]
            {
                // x           y           z           Texture(x, y)
                1.0f + x,   1.0f + y,   0.0f + z,      ux + uw,   uy + uh,
                0.0f + x,   0.0f + y,   0.0f + z,      ux,        uy,
                1.0f + x,   0.0f + y,   0.0f + z,      ux + uw,   uy,
                0.0f + x,   1.0f + y,   0.0f + z,      ux,        uy + uh
            });
            _indices.AddRange(new[]
            {
                0 + _indicesIndex, 1 + _indicesIndex, 2 + _indicesIndex,
                0 + _indicesIndex, 3 + _indicesIndex, 1 + _indicesIndex
            });

            _indicesIndex += 4;
        }
        private void GenerateMeshBack(int x, int y, int z)
        {
            UVTransform transform = _blocks[x, y, z].Texture.FrontTexture;

            float ux = transform.UvX;
            float uy = transform.UvY;
            float uw = transform.UvW;
            float uh = transform.UvH;

            z -= 1;

            _vertices.AddRange(new[]
            {
                // x           y           z           Texture(x, y)
                1.0f + x,   1.0f + y,   0.0f + z,      ux + uw,   uy + uh,
                0.0f + x,   0.0f + y,   0.0f + z,      ux,        uy,
                1.0f + x,   0.0f + y,   0.0f + z,      ux + uw,   uy,
                0.0f + x,   1.0f + y,   0.0f + z,      ux,        uy + uh
            });
            _indices.AddRange(new[]
            {
                3 + _indicesIndex, 2 + _indicesIndex, 1 + _indicesIndex,
                3 + _indicesIndex, 0 + _indicesIndex, 2 + _indicesIndex
            });

            _indicesIndex += 4;
        }
    }
}