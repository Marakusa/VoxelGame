using System;
using System.Collections.Generic;

namespace VoxelGame.Game
{
    public class Blocks
    {
        public static Blocks Instance;

        private readonly Dictionary<string, Block> _blocks = new();
        private readonly Block _nullBlock = new("null", "null", 100, false, new BlockTexture("null"), false);

        public Blocks()
        {
            Instance = this;
            _blocks = new();
            LoadBlocks();
        }

        private void LoadBlocks()
        {
            Block block = new("dirt",
                "Dirt",
                100,
                false,
                new("dirt"),
                false
            );
            _blocks.Add(block.BlockId, block);
        }

        public static Block Get(string name)
        {
            try
            {
                return Instance._blocks[name];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Instance._nullBlock;
            }
        }
    }
}