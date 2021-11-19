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
        public readonly UVTransform TopTexture;
        public readonly UVTransform BottomTexture;
        public readonly UVTransform LeftTexture;
        public readonly UVTransform RightTexture;
        public readonly UVTransform FrontTexture;
        public readonly UVTransform BackTexture;

        public BlockTexture(string texture)
        {
            UVTransform uvTexture = TextureManager.GetTexture(texture);

            TopTexture = uvTexture;
            BottomTexture = uvTexture;
            LeftTexture = uvTexture;
            RightTexture = uvTexture;
            FrontTexture = uvTexture;
            BackTexture = uvTexture;
        }

        public BlockTexture(string sides, string top, string bottom)
        {
            UVTransform uvSides = TextureManager.GetTexture(sides);
            UVTransform uvTop = TextureManager.GetTexture(top);
            UVTransform uvBottom = TextureManager.GetTexture(bottom);

            TopTexture = uvTop;
            BottomTexture = uvBottom;
            LeftTexture = uvSides;
            RightTexture = uvSides;
            FrontTexture = uvSides;
            BackTexture = uvSides;
        }

        public BlockTexture(string sides, string top, string bottom, string front)
        {
            UVTransform uvSides = TextureManager.GetTexture(sides);
            UVTransform uvTop = TextureManager.GetTexture(top);
            UVTransform uvBottom = TextureManager.GetTexture(bottom);
            UVTransform uvFront = TextureManager.GetTexture(front);

            TopTexture = uvTop;
            BottomTexture = uvBottom;
            LeftTexture = uvSides;
            RightTexture = uvSides;
            FrontTexture = uvFront;
            BackTexture = uvSides;
        }

        public BlockTexture(string top, string bottom, string front, string back, string left, string right)
        {
            UVTransform uvTop = TextureManager.GetTexture(top);
            UVTransform uvBottom = TextureManager.GetTexture(bottom);
            UVTransform uvFront = TextureManager.GetTexture(front);
            UVTransform uvBack = TextureManager.GetTexture(back);
            UVTransform uvLeft = TextureManager.GetTexture(left);
            UVTransform uvRight = TextureManager.GetTexture(right);

            TopTexture = uvTop;
            BottomTexture = uvBottom;
            LeftTexture = uvLeft;
            RightTexture = uvRight;
            FrontTexture = uvFront;
            BackTexture = uvBack;
        }
    }
}