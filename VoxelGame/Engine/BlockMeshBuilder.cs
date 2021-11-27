using System.Threading.Tasks;
using OpenTK.Mathematics;
using VoxelGame.Game;

namespace VoxelGame.Engine
{
    public delegate void MeshGenerationHandler(MeshGenerationEventArgs args);

    public static class BlockMeshBuilder
    {
        // TODO: Reduce vertex count if possible
        
        private const float UvPadding = 0.00625f;

        public static void GenerateMeshFront(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX + UvPadding / (1f / uv.UvW);
            float uy = uv.UvY + UvPadding / (1f / uv.UvW);
            float uw = uv.UvW - UvPadding / (1f / uv.UvW) * 2f;
            float uh = uv.UvH - UvPadding / (1f / uv.UvH) * 2f;

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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Front));
        }
        public static void GenerateMeshBack(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX + UvPadding / (1f / uv.UvW);
            float uy = uv.UvY + UvPadding / (1f / uv.UvW);
            float uw = uv.UvW - UvPadding / (1f / uv.UvW) * 2f;
            float uh = uv.UvH - UvPadding / (1f / uv.UvH) * 2f;

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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Back));
        }
        public static void GenerateMeshRight(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX + UvPadding / (1f / uv.UvW);
            float uy = uv.UvY + UvPadding / (1f / uv.UvW);
            float uw = uv.UvW - UvPadding / (1f / uv.UvW) * 2f;
            float uh = uv.UvH - UvPadding / (1f / uv.UvH) * 2f;

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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Right));
        }
        public static void GenerateMeshLeft(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX + UvPadding / (1f / uv.UvW);
            float uy = uv.UvY + UvPadding / (1f / uv.UvW);
            float uw = uv.UvW - UvPadding / (1f / uv.UvW) * 2f;
            float uh = uv.UvH - UvPadding / (1f / uv.UvH) * 2f;

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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Left));
        }
        public static void GenerateMeshTop(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX + UvPadding / (1f / uv.UvW);
            float uy = uv.UvY + UvPadding / (1f / uv.UvW);
            float uw = uv.UvW - UvPadding / (1f / uv.UvW) * 2f;
            float uh = uv.UvH - UvPadding / (1f / uv.UvH) * 2f;

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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Top));
        }
        public static void GenerateMeshBottom(float x, float y, float z, UVTransform uv, MeshGenerationHandler callback)
        {
            float ux = uv.UvX + UvPadding / (1f / uv.UvW);
            float uy = uv.UvY + UvPadding / (1f / uv.UvW);
            float uw = uv.UvW - UvPadding / (1f / uv.UvW) * 2f;
            float uh = uv.UvH - UvPadding / (1f / uv.UvH) * 2f;

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

            callback.Invoke(new(points, uvs, indices, x, y, z, FaceSide.Bottom));
        }
    }

    public class MeshGenerationEventArgs
    {
        public Vector3[] points;
        public Vector2[] uvs;
        public uint[] indices;
        public float x, y, z;
        public FaceSide side;

        public MeshGenerationEventArgs(Vector3[] points, Vector2[] uvs, uint[] indices, float x, float y, float z, FaceSide side)
        {
            this.points = points;
            this.uvs = uvs;
            this.indices = indices;
            this.x = x;
            this.y = y;
            this.z = z;
            this.side = side;
        }
    }
}
