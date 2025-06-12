namespace VoxelGame.Game
{
    public class Block : Item
    {
        public int BlockId;
        public readonly float Hardness;
        public readonly BlockTexture Texture;
        public readonly bool IsRotationCameraRelative;
        public readonly bool HasGravity;
        public readonly bool Holdable;

        public Block(LoadedBlock loadedBlock)
        {
            ItemId = loadedBlock.id;
            ItemName = loadedBlock.name;
            MaxStack = loadedBlock.max_stack;
            Hardness = loadedBlock.hardness;
            IsTransparent = loadedBlock.transparent;
            IsRotationCameraRelative = loadedBlock.camera_relative;
            Holdable = loadedBlock.holdable;

            switch (loadedBlock.texture.Length)
            {
                case 0:
                    Texture = new("null");
                    break;
                case 1 or 2:
                    Texture = new(loadedBlock.texture[0]);
                    break;
                case 3:
                    Texture = new(
                        loadedBlock.texture[0], 
                        loadedBlock.texture[1], 
                        loadedBlock.texture[2]);
                    break;
                case 4 or 5:
                    Texture = new(
                        loadedBlock.texture[0], 
                        loadedBlock.texture[1], 
                        loadedBlock.texture[2], 
                        loadedBlock.texture[3]);
                    break;
                case 6:
                    Texture = new(
                        loadedBlock.texture[0], 
                        loadedBlock.texture[1], 
                        loadedBlock.texture[2], 
                        loadedBlock.texture[3], 
                        loadedBlock.texture[4], 
                        loadedBlock.texture[5]);
                    break;
                default:
                    Texture = new("null");
                    break;
            }
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

        public BlockTexture(string top, string bottom, string front, string back, string right, string left)
        {
            UVTransform uvTop = TextureManager.GetTexture(top);
            UVTransform uvBottom = TextureManager.GetTexture(bottom);
            UVTransform uvFront = TextureManager.GetTexture(front);
            UVTransform uvBack = TextureManager.GetTexture(back);
            UVTransform uvRight = TextureManager.GetTexture(right);
            UVTransform uvLeft = TextureManager.GetTexture(left);

            TopTexture = uvTop;
            BottomTexture = uvBottom;
            LeftTexture = uvLeft;
            RightTexture = uvRight;
            FrontTexture = uvFront;
            BackTexture = uvBack;
        }
    }
}