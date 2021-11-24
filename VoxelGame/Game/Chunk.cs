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

        public void DestroyBlock(Vector3 position)
        {
            if (position.X >= 0 && position.Y >= 0 && position.Z >= 0
                && position.X < Width && position.Y < Height && position.Z < Width)
            {
                Console.WriteLine(_blocks[(int)position.X, (int)position.Y, (int)position.Z].BlockId);
                Console.WriteLine(new Vector3((int)position.X, (int)position.Y, (int)position.Z).ToString());

                _blocks[(int)position.X, (int)position.Y, (int)position.Z] = null;
                
                GenerateMesh();
            }
        }

        public Vector3 CheckRaycastHitPoint(Vector3 start, Vector3 direction, float length)
        {
            direction.Normalize();
            Vector3 end = direction * length;
            
            Console.WriteLine("----------------");
            Console.WriteLine(new Vector3((float)Math.Round(direction.X), (float)Math.Round(direction.Y), (float)Math.Round(direction.Z)).ToString());
            Console.WriteLine(direction.ToString());

            int mapX = (int)start.X;
            int mapY = (int)start.Y;
            int mapZ = (int)start.Z;
            
            double toDistX;
            double toDistY;
            double toDistZ;
            
            double deltaDistX = (direction.X == 0) ? 1e30 : (1 / direction.X);
            double deltaDistY = (direction.Y == 0) ? 1e30 : (1 / direction.Y);
            double deltaDistZ = (direction.Z == 0) ? 1e30 : (1 / direction.Z);
            
            int stepX;
            int stepY;
            int stepZ;

            bool hit = false;
            
            if (direction.X < 0)
            {
                stepX = -1;
                toDistX = ((int)start.X - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                toDistX = (mapX + 1.0 - (int)start.X) * deltaDistX;
            }
            
            if (direction.Y < 0)
            {
                stepY = -1;
                toDistY = ((int)start.Y - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                toDistY = (mapY + 1.0 - (int)start.Y) * deltaDistY;
            }
            
            if (direction.Z < 0)
            {
                stepZ = -1;
                toDistZ = ((int)start.Z - mapZ) * deltaDistZ;
            }
            else
            {
                stepZ = 1;
                toDistZ = (mapZ + 1.0 - (int)start.Z) * deltaDistZ;
            }
            
            while (!hit)
            {
                if (toDistX < toDistY && toDistX < toDistZ)
                {
                    toDistX += deltaDistX;
                    mapX += stepX;
                }
                else if (toDistY < toDistZ)
                {
                    toDistY += deltaDistY;
                    mapY += stepY;
                }
                else
                {
                    toDistZ += deltaDistZ;
                    mapZ += stepZ;
                }

                if (Vector3.Distance(start, new(mapX, mapY, mapZ)) > length)
                {
                    return Vector3.NegativeInfinity;
                }

                if (mapX >= Position.X && mapY >= 0 && mapZ >= Position.Y
                    && mapX < Position.X + Width && mapY < Height && mapZ < Position.Y + Width)
                {
                    if (_blocks[mapX - (int)Position.X, mapY, mapZ - (int)Position.Y] != null)
                    {
                        hit = true;
                    }
                }
            }
            
            return new(mapX, mapY, mapZ);
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
                            if (!HasBlock(x, y, z + 1)) GenerateMeshBack(x, y, z);
                            if (!HasBlock(x, y, z - 1)) GenerateMeshFront(x, y, z);
                            if (!HasBlock(x + 1, y, z)) GenerateMeshRight(x, y, z);
                            if (!HasBlock(x - 1, y, z)) GenerateMeshLeft(x, y, z);
                            if (!HasBlock(x, y + 1, z)) GenerateMeshTop(x, y, z);
                            if (!HasBlock(x, y - 1, z)) GenerateMeshBottom(x, y, z);
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

        private bool HasBlock(int x, int y, int z)
        {
            if (y == -1 || y == Height)
                return false;

            if (x >= 0 && y >= 0 && z >= 0 && x < Width && y < Height && z < Width)
                return _blocks[x, y, z] != null && _blocks[x, y, z].BlockId != "air";
            
            if ((x >= -1 && y >= 0 && z >= -1) || (x <= Width && y < Height && z <= Width))
            {
                int noiseY = Noise.GetNoise(x + (int)Math.Round(Position.X), z + (int)Math.Round(Position.Y));
                return noiseY >= y;
            }

            return false;
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
                    if (HasBlock(vX, vY, vZ)
                        || HasBlock(vX - 1, vY, vZ)
                        || HasBlock(vX, vY, vZ - 1)
                        || HasBlock(vX - 1, vY, vZ - 1)
                        
                        || HasBlock(vX, vY + 1, vZ)
                        || HasBlock(vX - 1, vY + 1, vZ - 1))
                    {
                        lightLevel -= 0.35f;
                    }
                    break;
                case FaceSide.Front:
                    if (HasBlock(vX, vY - 1, vZ - 1)
                        || HasBlock(vX - 1, vY - 1, vZ - 1))
                    {
                        lightLevel -= 0.35f;
                    }
                    break;
                case FaceSide.Left:
                    if (HasBlock(vX - 1, vY - 1, vZ)
                        || HasBlock(vX - 1, vY - 1, vZ - 1))
                    {
                        lightLevel -= 0.35f;
                    }
                    break;
                case FaceSide.Right:
                    if (HasBlock(vX, vY - 1, vZ)
                        || HasBlock(vX, vY - 1, vZ - 1))
                    {
                        lightLevel -= 0.35f;
                    }
                    break;
                case FaceSide.Back:
                    if (HasBlock(vX, vY - 1, vZ)
                        || HasBlock(vX - 1, vY - 1, vZ))
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