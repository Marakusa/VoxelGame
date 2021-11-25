using OpenTK.Mathematics;
using VoxelGame.Engine;

namespace VoxelGame.Game
{
    public class HighlightBlock
    {
        public float[] BlockHighlightVertices =
        {
            // 0
            0f, 0f, 0f, 0f, 0f, 1f,
            // 1
            1f, 0f, 0f, 1f, 0f, 1f,
            // 2
            1f, 1f, 0f, 1f, 1f, 1f,
            // 3
            0f, 1f, 0f, 0f, 1f, 1f,
            
            // 4
            0f, 0f, 1f, 0f, 0f, 1f,
            // 5
            1f, 0f, 1f, 1f, 0f, 1f,
            // 6
            1f, 1f, 1f, 1f, 1f, 1f,
            // 7
            0f, 1f, 1f, 0f, 1f, 1f
        };
        public uint[] BlockHighlightIndices =
        {
            2, 1, 0,
            3, 2, 0,
            
            3, 0, 4,
            7, 3, 4,
            
            7, 4, 5,
            6, 7, 5,
            
            6, 5, 1,
            2, 6, 1,
            
            6, 2, 3,
            7, 6, 3,
            
            0, 1, 5,
            0, 5, 4,
        };
        public VertexBuffer BlockHighlightVb;
        public IndexBuffer BlockHighlightIb;
        public Vector3 BlockHighlightPoint = Vector3.Zero;
    }
}
