using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Engine;

namespace VoxelGame.Game
{
    public class Chunk
    {
        public Vector2 Position;

        public readonly int Width = 64, Height = 128;

        private Block[,,] _blocks;

        public List<float> Vertices = new();
        public List<int> Indices = new();

        public delegate void GeneratedHandler(object sender, float[] vertices, int[] indices);

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
            if (position.X >= Position.X && position.Y >= 0 && position.Z >= Position.Y
                && position.X < Position.X + Width && position.Y < Height && position.Z < Position.Y + Width)
            {
                Console.WriteLine(_blocks[(int)position.X, (int)position.Y, (int)position.Z].BlockId);

                _blocks[(int)position.X, (int)position.Y, (int)position.Z] = null;
                
                //GenerateMesh();
            }
        }

        public Vector3 CheckRaycastHitPoint(Vector3 start, Vector3 direction, float length)
        {
            direction.Normalize();
            Vector3 end = direction * length;
            
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
                    if (_blocks[mapX, mapY, mapZ] != null)
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

                        if (noiseHeight >= 0 && noiseHeight < Height)
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

        private void GenerateMesh()
        {
            Vertices.Clear();
            Indices.Clear();
            
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
            
            Generated?.Invoke(this, Vertices.ToArray(), Indices.ToArray());
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

        private int _indicesIndex = 0;
        private void AddMesh(Vector3[] vertices, Vector2[] uvs, int[] indices, int blockX, int blockY, int blockZ, FaceSide side)
        {
            int i = 0;
            foreach (var vertex in vertices)
            {
                float lightLevel = CalculateLightLevel(vertex, side, blockX, blockY, blockZ);
                
                Vertices.AddRange(new[]
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
                Indices.Add(index + _indicesIndex);
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
            int[] indices =
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
            int[] indices =
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
            int[] indices =
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
            int[] indices =
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
            int[] indices =
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
            int[] indices =
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