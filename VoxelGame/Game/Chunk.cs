using System.Collections.Generic;

namespace VoxelGame.Game
{
    public class Chunk
    {
        private const int Width = 16, Height = 3;
        
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
        }
        private void GenerateMesh()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Width; z++)
                    {
                        GenerateMeshFront(x, y, z);
                    }
                }   
            }
            
            Generated?.Invoke(this, _vertices.ToArray(), _indices.ToArray());
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
                // x    y    z    Texture(x, y)
                0.0f + x, 1.0f + y, 0.0f + z, ux,      uy + uh,
                1.0f + x, 0.0f + y, 0.0f + z, ux + uw, uy,
                0.0f + x, 0.0f + y, 0.0f + z, ux,      uy,
                1.0f + x, 1.0f + y, 0.0f + z, ux + uw, uy + uh,
            });
            _indices.AddRange(new[]
            {
                0, 1, 2,
                0, 3, 1
            });
        }
    }
}