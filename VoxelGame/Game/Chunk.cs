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

        public float[] Vertices = Array.Empty<float>();
        public uint[] Indices = Array.Empty<uint>();

        private readonly List<float> _tempVertices = new();
        private readonly List<uint> _tempIndices = new();

        public VertexBuffer Vb;
        public IndexBuffer Ib;
        
        public delegate void GeneratedHandler(object sender, float[] vertices, uint[] indices);

        public event GeneratedHandler Generated;

        public Chunk(int x, int y, int w, int h)
        {
            Width = w;
            Height = h;
            _blocks = new Block[Width, Height, Width];
            Position = new(x, y);
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
                            if (!IsTransparentBlock(x, y, z + 1)) GenerateMeshBack(x, y, z);
                            if (!IsTransparentBlock(x, y, z - 1)) GenerateMeshFront(x, y, z);
                            if (!IsTransparentBlock(x + 1, y, z)) GenerateMeshRight(x, y, z);
                            if (!IsTransparentBlock(x - 1, y, z)) GenerateMeshLeft(x, y, z);
                            if (!IsTransparentBlock(x, y + 1, z)) GenerateMeshTop(x, y, z);
                            if (!IsTransparentBlock(x, y - 1, z)) GenerateMeshBottom(x, y, z);
                        }
                    }
                }
            }
            
            Vertices = _tempVertices.ToArray();
            Indices = _tempIndices.ToArray();
            
            _tempVertices.Clear();
            _tempIndices.Clear();

            if (Vb == null)
                Vb = new VertexBuffer(Vertices, Vertices.Length * sizeof(float));
            else
            {
                Vb.Unbind();
                Vb.SetBufferData(Vertices, Vertices.Length * sizeof(float));
            }
            
            if (Ib == null)
                Ib = new IndexBuffer(Indices, Indices.Length * sizeof(uint));
            else
            {
                Ib.Unbind();
                Ib.SetBufferData(Indices, Indices.Length * sizeof(uint));
            }
            
            Generated?.Invoke(this, Vertices, Indices);
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
                    lightLevel
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

        private void GenerateMeshFront(int x, int y, int z)
        {
            UVTransform transform = _blocks[x, y, z].Texture.FrontTexture;

            float ux = transform.UvX;
            float uy = transform.UvY;
            float uw = transform.UvW;
            float uh = transform.UvH;

            Vector3[] points =
            {
                new(0.0f + x, 1.0f + y, 0.0f + z),
                new(1.0f + x, 0.0f + y, 0.0f + z),
                new(0.0f + x, 0.0f + y, 0.0f + z),
                new(1.0f + x, 1.0f + y, 0.0f + z)
            };
            Vector2[] uvs =
            {
                new(ux + uw, uy),
                new(ux, uy + uh),
                new(ux + uw, uy + uh),
                new(ux, uy)
            };
            uint[] indices =
            {
                0, 1, 2,
                0, 3, 1
            };

            AddMesh(points, uvs, indices, x, y, z, FaceSide.Front);
        }
        private void GenerateMeshBack(int x, int y, int z)
        {
            UVTransform transform = _blocks[x, y, z].Texture.BackTexture;

            float ux = transform.UvX;
            float uy = transform.UvY;
            float uw = transform.UvW;
            float uh = transform.UvH;

            z += 1;

            Vector3[] points =
            {
                new(0.0f + x, 1.0f + y, 0.0f + z),
                new(1.0f + x, 0.0f + y, 0.0f + z),
                new(0.0f + x, 0.0f + y, 0.0f + z),
                new(1.0f + x, 1.0f + y, 0.0f + z)
            };
            Vector2[] uvs =
            {
                new(ux, uy),
                new(ux + uw, uy + uh),
                new(ux, uy + uh),
                new(ux + uw, uy)
            };
            uint[] indices =
            {
                3, 2, 1,
                3, 0, 2
            };

            AddMesh(points, uvs, indices, x, y, z, FaceSide.Back);
        }
        private void GenerateMeshRight(int x, int y, int z)
        {
            UVTransform transform = _blocks[x, y, z].Texture.RightTexture;

            float ux = transform.UvX;
            float uy = transform.UvY;
            float uw = transform.UvW;
            float uh = transform.UvH;

            x += 1;

            Vector3[] points =
            {
                new(0.0f + x, 1.0f + y, 1.0f + z),
                new(0.0f + x, 0.0f + y, 0.0f + z),
                new(0.0f + x, 0.0f + y, 1.0f + z),
                new(0.0f + x, 1.0f + y, 0.0f + z)
            };
            Vector2[] uvs =
            {
                new(ux, uy),
                new(ux + uw, uy + uh),
                new(ux, uy + uh),
                new(ux + uw, uy),
            };
            uint[] indices =
            {
                3, 2, 1,
                3, 0, 2
            };

            AddMesh(points, uvs, indices, x, y, z, FaceSide.Right);
        }
        private void GenerateMeshLeft(int x, int y, int z)
        {
            UVTransform transform = _blocks[x, y, z].Texture.LeftTexture;

            float ux = transform.UvX;
            float uy = transform.UvY;
            float uw = transform.UvW;
            float uh = transform.UvH;

            Vector3[] points =
            {
                new(0.0f + x, 1.0f + y, 1.0f + z),
                new(0.0f + x, 0.0f + y, 0.0f + z),
                new(0.0f + x, 0.0f + y, 1.0f + z),
                new(0.0f + x, 1.0f + y, 0.0f + z)
            };
            Vector2[] uvs =
            {
                new(ux + uw, uy),
                new(ux, uy + uh),
                new(ux + uw, uy + uh),
                new(ux, uy),
            };
            uint[] indices =
            {
                0, 1, 2,
                0, 3, 1
            };

            AddMesh(points, uvs, indices, x, y, z, FaceSide.Left);
        }
        private void GenerateMeshTop(int x, int y, int z)
        {
            UVTransform transform = _blocks[x, y, z].Texture.TopTexture;

            float ux = transform.UvX;
            float uy = transform.UvY;
            float uw = transform.UvW;
            float uh = transform.UvH;

            y += 1;

            Vector3[] points =
            {
                new(0.0f + x, 0.0f + y, 1.0f + z),
                new(1.0f + x, 0.0f + y, 0.0f + z),
                new(0.0f + x, 0.0f + y, 0.0f + z),
                new(1.0f + x, 0.0f + y, 1.0f + z)
            };
            Vector2[] uvs =
            {
                new(ux + uw, uy),
                new(ux, uy + uh),
                new(ux + uw, uy + uh),
                new(ux, uy),
            };
            uint[] indices =
            {
                0, 1, 2,
                0, 3, 1
            };

            AddMesh(points, uvs, indices, x, y, z, FaceSide.Top);
        }
        private void GenerateMeshBottom(int x, int y, int z)
        {
            UVTransform transform = _blocks[x, y, z].Texture.BottomTexture;

            float ux = transform.UvX;
            float uy = transform.UvY;
            float uw = transform.UvW;
            float uh = transform.UvH;

            Vector3[] points =
            {
                new(0.0f + x, 0.0f + y, 1.0f + z),
                new(1.0f + x, 0.0f + y, 0.0f + z),
                new(0.0f + x, 0.0f + y, 0.0f + z),
                new(1.0f + x, 0.0f + y, 1.0f + z)
            };
            Vector2[] uvs =
            {
                new(ux, uy),
                new(ux + uw, uy + uh),
                new(ux, uy + uh),
                new(ux + uw, uy),
            };
            uint[] indices =
            {
                3, 2, 1,
                3, 0, 2
            };

            AddMesh(points, uvs, indices, x, y, z, FaceSide.Bottom);
        }
    }

    public enum FaceSide
    {
        Top, Bottom, Left, Right, Front, Back
    }
}