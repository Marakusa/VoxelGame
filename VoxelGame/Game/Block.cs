namespace VoxelGame.Game
{
    public class Block
    {
        public readonly string BlockId;
        public readonly string BlockName;
        public readonly int MaxStack;
        public readonly bool IsTransparent;
        public readonly BlockTexture Texture;
        public readonly bool IsRotationCameraRelative;
        
        /// <summary>
        /// Creates a block.
        /// </summary>
        /// <param name="blockId">Blocks id (i.e. glass_block).</param>
        /// <param name="blockName">Blocks name (i.e. Glass Block).</param>
        /// <param name="maxStack">Blocks maximum stack size.</param>
        /// <param name="isTransparent">Does block have transparency.</param>
        /// <param name="texture">Blocks textures name (i.e. glass).</param>
        public Block(string blockId, string blockName, int maxStack, bool isTransparent, BlockTexture texture, bool isRotationCameraRelative)
        {
            BlockId = blockId;
            BlockName = blockName;
            MaxStack = maxStack;
            IsTransparent = isTransparent;
            Texture = texture;
            IsRotationCameraRelative = isRotationCameraRelative;
        }
    }

    public class BlockTexture
    {
        public readonly string TopTexture;
        public readonly string BottomTexture;
        public readonly string LeftTexture;
        public readonly string RightTexture;
        public readonly string FrontTexture;
        public readonly string BackTexture;

        public BlockTexture(string texture)
        {
            TopTexture = texture;
            BottomTexture = texture;
            LeftTexture = texture;
            RightTexture = texture;
            FrontTexture = texture;
            BackTexture = texture;
        }
        
        public BlockTexture(string sides, string top, string bottom)
        {
            TopTexture = top;
            BottomTexture = bottom;
            LeftTexture = sides;
            RightTexture = sides;
            FrontTexture = sides;
            BackTexture = sides;
        }
        
        public BlockTexture(string sides, string top, string bottom, string front)
        {
            TopTexture = top;
            BottomTexture = bottom;
            LeftTexture = sides;
            RightTexture = sides;
            FrontTexture = front;
            BackTexture = sides;
        }
        
        public BlockTexture(string top, string bottom, string front, string back, string left, string right)
        {
            TopTexture = top;
            BottomTexture = bottom;
            LeftTexture = left;
            RightTexture = right;
            FrontTexture = front;
            BackTexture = back;
        }
    }
}