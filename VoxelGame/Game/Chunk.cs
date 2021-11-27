using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Engine;

namespace VoxelGame.Game
{
    public class Chunk
    {
        public Vector2 Position;

        public readonly int Width = 2, Height = 128;

        private Block[,,] _blocks;
        
        private Mesh mesh;

        public bool IsGenerated { get; private set; }
        public bool FirstGeneration { get; private set; }

        private readonly List<float> _tempVertices = new();
        private readonly List<uint> _tempIndices = new();

        public delegate void GeneratedHandler(object sender, float[] vertices, uint[] indices);

        public event GeneratedHandler Generated;
        
        public event GeneratedHandler ChunkSpawned;

        public Chunk(int x, int y, int w, int h)
        {
            mesh = new();
            Width = w;
            Height = h;
            _blocks = new Block[Width, Height, Width];
            Position = new(x, y);
            FirstGeneration = true;
        }

        public void DestroyBlock(int x, int y, int z)
        {
            if (x >= 0 && y >= 0 && z >= 0
                && x < Width && y < Height && z < Width
                && _blocks[x, y, z] != null && _blocks[x, y, z].Hardness >= 0)
            {
                _blocks[x, y, z] = null;
                
                GenerateMesh();
            }
        }

        public void PlaceBlock(int x, int y, int z, Block b)
        {
            if (x >= 0 && y >= 0 && z >= 0
                && x < Width && y < Height && z < Width
                && _blocks[x, y, z] == null)
            {
                if (b.HasGravity)
                {
                    for (; y > 0; y--)
                    {
                        if (_blocks[x, y, z] == null && _blocks[x, y - 1, z] != null)
                        {
                            _blocks[x, y, z] = b;
                            GenerateMesh();
                            break;
                        }
                    }
                }
                else
                {
                    _blocks[x, y, z] = b;
                    GenerateMesh();
                }
            }
        }

        public void RegenerateMesh()
        {
            GenerateMesh();
        }

        public void Generate()
        {
            IsGenerated = false;
            
            var noiseData = Noise.GetChunkNoise((int)Math.Round(Position.X), (int)Math.Round(Position.Y));
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int z = 0; z < Width; z++)
                    {
                        int noiseHeight = noiseData[x, z];

                        if (noiseHeight >= 0)
                        {
                            if (y == noiseHeight)
                                _blocks[x, y, z] = Blocks.Get("grass_block");
                            else if (y < noiseHeight && y > noiseHeight - 4)
                                _blocks[x, y, z] = Blocks.Get("dirt");
                            else if (y <= noiseHeight - 4)
                                _blocks[x, y, z] = Blocks.Get("stone");
                        }
                    }
                }
            }
            
            GenerateMesh();
        }

        private uint _indicesIndex = 0;
        
        private void GenerateMesh()
        {
            IsGenerated = false;

            _indicesIndex = 0;
            
            _tempVertices.Clear();
            _tempIndices.Clear();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Width; z++)
                    {
                        if (_blocks[x, y, z] != null)
                        {
                            if (!IsTransparentBlock(x, y, z + 1)) BlockMeshBuilder.GenerateMeshBack(x, y, z, _blocks[x, y, z].Texture.BackTexture, MeshGeneratedCallback);
                            if (!IsTransparentBlock(x, y, z - 1)) BlockMeshBuilder.GenerateMeshFront(x, y, z, _blocks[x, y, z].Texture.FrontTexture, MeshGeneratedCallback);
                            if (!IsTransparentBlock(x + 1, y, z)) BlockMeshBuilder.GenerateMeshRight(x, y, z, _blocks[x, y, z].Texture.RightTexture, MeshGeneratedCallback);
                            if (!IsTransparentBlock(x - 1, y, z)) BlockMeshBuilder.GenerateMeshLeft(x, y, z, _blocks[x, y, z].Texture.LeftTexture, MeshGeneratedCallback);
                            if (!IsTransparentBlock(x, y + 1, z)) BlockMeshBuilder.GenerateMeshTop(x, y, z, _blocks[x, y, z].Texture.TopTexture, MeshGeneratedCallback);
                            if (!IsTransparentBlock(x, y - 1, z)) BlockMeshBuilder.GenerateMeshBottom(x, y, z, _blocks[x, y, z].Texture.BottomTexture, MeshGeneratedCallback);
                        }
                    }
                }
            }

            mesh.SetData(_tempVertices.ToArray(), _tempIndices.ToArray(), delegate(object sender, EventArgs args)
            {
                _tempVertices.Clear();
                _tempIndices.Clear();
        
                IsGenerated = true;

                Generated?.Invoke(this, mesh.Vertices, mesh.Indices);

                if (FirstGeneration)
                {
                    ChunkSpawned?.Invoke(this, mesh.Vertices, mesh.Indices);
                    FirstGeneration = false;
                }
                else
                {
                    SetBuffers();
                }
            });
        }

        private void MeshGeneratedCallback(MeshGenerationEventArgs args)
        {
            AddMesh(args.points, args.uvs, args.indices, (int)args.x, (int)args.y, (int)args.z, args.side);
        }

        public void DeleteBuffers()
        {
            mesh.DeleteBuffers();
        }

        public bool HasBlock(int x, int y, int z)
        {
            if (x >= 0 && y >= 0 && z >= 0 && x < Width && y < Height && z < Width)
                return _blocks[x, y, z] != null;
            
            return false;
        }
        
        private bool IsTransparentBlock(int x, int y, int z)
        {
            if (y == -1 || y == Height)
                return false;

            if (x >= 0 && y >= 0 && z >= 0 && x < Width && y < Height && z < Width)
                return _blocks[x, y, z] != null && _blocks[x, y, z].ItemId != "air";
            
            int cx = (int)Math.Floor(x + Position.X);
            int cy = y;
            int cz = (int)Math.Floor(z + Position.Y);
                
            var c = ChunkManager.GetChunkByPoint(new(cx, cy, cz));

            if (c != null)
                return c._blocks[cx - (int)Math.Floor(c.Position.X), cy, cz - (int)Math.Floor(c.Position.Y)] != null && c._blocks[cx - (int)Math.Floor(c.Position.X), cy, cz - (int)Math.Floor(c.Position.Y)].ItemId != "air";
                
            int noiseY = Noise.GetNoise(x + (int)Math.Round(Position.X), z + (int)Math.Round(Position.Y));
            return noiseY >= y;
        }

        private void AddMesh(Vector3[] vertices, Vector2[] uvs, uint[] indices, int blockX, int blockY, int blockZ, FaceSide side)
        {
            int i = 0;
            foreach (var vertex in vertices)
            {
                float lightLevel = CalculateLightLevel(vertex, side, blockX, blockY, blockZ);
                
                _tempVertices.AddRange(new[]
                {
                    vertex.X + (int)Math.Round(Position.X), 
                    vertex.Y, 
                    vertex.Z + (int)Math.Round(Position.Y),
                    uvs[i].X, 
                    uvs[i].Y, 
                    lightLevel,
                    lightLevel,
                    lightLevel,
                    1.0f
                });
                i++;
            }

            foreach (var index in indices)
            {
                _tempIndices.Add(index + _indicesIndex);
            }

            _indicesIndex += 4;
        }

        private float CalculateLightLevel(Vector3 vertex, FaceSide side, int blockX, int blockY, int blockZ)
        {
            float lightLevel = 1f;

            int vX = (int)Math.Round(vertex.X), vY = (int)Math.Round(vertex.Y), vZ = (int)Math.Round(vertex.Z);
            
            switch (side)
            {
                case FaceSide.Bottom:
                    lightLevel -= 0.8f;
                    break;
                case FaceSide.Top:
                    if (IsTransparentBlock(vX, vY, vZ)
                        || IsTransparentBlock(vX - 1, vY, vZ)
                        || IsTransparentBlock(vX, vY, vZ - 1)
                        || IsTransparentBlock(vX - 1, vY, vZ - 1)
                        
                        || IsTransparentBlock(vX, vY + 1, vZ)
                        || IsTransparentBlock(vX - 1, vY + 1, vZ - 1))
                    {
                        lightLevel -= 0.35f;
                    }
                    break;
                case FaceSide.Front:
                    if (IsTransparentBlock(vX, vY - 1, vZ - 1)
                        || IsTransparentBlock(vX - 1, vY - 1, vZ - 1))
                    {
                        lightLevel -= 0.35f;
                    }
                    break;
                case FaceSide.Left:
                    if (IsTransparentBlock(vX - 1, vY - 1, vZ)
                        || IsTransparentBlock(vX - 1, vY - 1, vZ - 1))
                    {
                        lightLevel -= 0.35f;
                    }
                    break;
                case FaceSide.Right:
                    if (IsTransparentBlock(vX, vY - 1, vZ)
                        || IsTransparentBlock(vX, vY - 1, vZ - 1))
                    {
                        lightLevel -= 0.35f;
                    }
                    break;
                case FaceSide.Back:
                    if (IsTransparentBlock(vX, vY - 1, vZ)
                        || IsTransparentBlock(vX - 1, vY - 1, vZ))
                    {
                        lightLevel -= 0.35f;
                    }
                    break;
            }

            return Math.Clamp(lightLevel, 0f, 1f);
        }

        public VertexBuffer GetVertexBuffer()
        {
            return mesh.Vb;
        }
        public IndexBuffer GetIndexBuffer()
        {
            return mesh.Ib;
        }
        public void SetBuffers()
        {
            mesh.SetBuffers();
        }

        public void Unload()
        {
            IsGenerated = false;
            var ch = ChunkManager.GetChunks();
            ch.Remove(new(Position.X / Width, Position.Y / Width));
        }
    }

    public enum FaceSide
    {
        Top, Bottom, Left, Right, Front, Back
    }
}