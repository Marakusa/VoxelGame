using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Engine;

namespace VoxelGame.Game
{
    public class HighlightBlock
    {
        public Mesh mesh;
        
        private readonly List<float> _tempVertices = new();
        private readonly List<uint> _tempIndices = new();
        //public VertexBuffer BlockHighlightVb;
        //public IndexBuffer BlockHighlightIb;
        public Vector3 Position = Vector3.Zero;

        public HighlightBlock()
        {
            mesh = new();
            Generate();
        }
        
        public void Generate()
        {
            _indicesIndex = 0;

            _tempVertices.Clear();
            _tempIndices.Clear();
            
            Block highlightBlock = Blocks.Get("dirt");

            float hx = Position.X,
                hy = Position.Y,
                hz = Position.Z;
            
            GenerateMeshBack(hx, hy, hz, highlightBlock.Texture.BackTexture, MeshGeneratedCallback);
            GenerateMeshFront(hx, hy, hz, highlightBlock.Texture.FrontTexture, MeshGeneratedCallback);
            GenerateMeshRight(hx, hy, hz, highlightBlock.Texture.RightTexture, MeshGeneratedCallback);
            GenerateMeshLeft(hx, hy, hz, highlightBlock.Texture.LeftTexture, MeshGeneratedCallback);
            GenerateMeshTop(hx, hy, hz, highlightBlock.Texture.TopTexture, MeshGeneratedCallback);
            GenerateMeshBottom(hx, hy, hz, highlightBlock.Texture.BottomTexture, MeshGeneratedCallback);

            mesh.SetData(_tempVertices.ToArray(), _tempIndices.ToArray());
        
            _tempVertices.Clear();
            _tempIndices.Clear();
        }

        private void MeshGeneratedCallback(MeshGenerationEventArgs args)
        {
            AddMesh(args.points, args.uvs, args.indices, args.x, args.y, args.z, args.side);
        }

        private uint _indicesIndex;
        private void AddMesh(Vector3[] vertices, Vector2[] uvs, uint[] indices, float blockX, float blockY, float blockZ, FaceSide side)
        {
            int i = 0;
            
            foreach (var vertex in vertices)
            {
                _tempVertices.AddRange(new[]
                {
                    vertex.X, 
                    vertex.Y, 
                    vertex.Z,
                    uvs[i].X, 
                    uvs[i].Y, 
                    1.0f,
                    1.0f,
                    1.0f,
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

        private static void GenerateMeshFront(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX;
            float uy = uv.UvY;
            float uw = uv.UvW;
            float uh = uv.UvH;

            Vector3[] points =
            {
                new(-0.01f + x, 1.01f + y, -0.01f + z),
                new(1.01f + x, -0.01f + y, -0.01f + z),
                new(-0.01f + x, -0.01f + y, -0.01f + z),
                new(1.01f + x, 1.01f + y, -0.01f + z)
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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Front));
        }
        private static void GenerateMeshBack(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX;
            float uy = uv.UvY;
            float uw = uv.UvW;
            float uh = uv.UvH;

            z += 1;

            Vector3[] points =
            {
                new(-0.01f + x, 1.01f + y, 0.01f + z),
                new(1.01f + x, -0.01f + y, 0.01f + z),
                new(-0.01f + x, -0.01f + y, 0.01f + z),
                new(1.01f + x, 1.01f + y, 0.01f + z)
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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Back));
        }
        private static void GenerateMeshRight(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX;
            float uy = uv.UvY;
            float uw = uv.UvW;
            float uh = uv.UvH;

            x += 1;

            Vector3[] points =
            {
                new(0.01f + x, 1.01f + y, 1.01f + z),
                new(0.01f + x, -0.01f + y, -0.01f + z),
                new(0.01f + x, -0.01f + y, 1.01f + z),
                new(0.01f + x, 1.01f + y, -0.01f + z)
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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Right));
        }
        private static void GenerateMeshLeft(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX;
            float uy = uv.UvY;
            float uw = uv.UvW;
            float uh = uv.UvH;

            Vector3[] points =
            {
                new(-0.01f + x, 1.01f + y, 1.01f + z),
                new(-0.01f + x, -0.01f + y, -0.01f + z),
                new(-0.01f + x, -0.01f + y, 1.01f + z),
                new(-0.01f + x, 1.01f + y, -0.01f + z)
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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Left));
        }
        private static void GenerateMeshTop(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX;
            float uy = uv.UvY;
            float uw = uv.UvW;
            float uh = uv.UvH;

            y += 1;

            Vector3[] points =
            {
                new(-0.01f + x, 0.01f + y, 1.01f + z),
                new(1.01f + x, 0.01f + y, -0.01f + z),
                new(-0.01f + x, 0.01f + y, -0.01f + z),
                new(1.01f + x, 0.01f + y, 1.01f + z)
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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Top));
        }
        private static void GenerateMeshBottom(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX;
            float uy = uv.UvY;
            float uw = uv.UvW;
            float uh = uv.UvH;

            Vector3[] points =
            {
                new(-0.01f + x, -0.01f + y, 1.01f + z),
                new(1.01f + x, -0.01f + y, -0.01f + z),
                new(-0.01f + x, -0.01f + y, -0.01f + z),
                new(1.01f + x, -0.01f + y, 1.01f + z)
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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Bottom));
        }
    }
}
