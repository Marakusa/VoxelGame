using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VoxelGame.Game
{
    public static class Blocks
    {
        private static readonly Dictionary<string, Block> BlockList = new();

        public static void Initialize()
        {
            LoadBlocks();
        }

        private static void LoadBlocks()
        {
            foreach (var dataFile in Directory.GetFiles("assets/data/blocks/"))
            {
                if (Path.GetExtension(dataFile) == ".json")
                {
                    var loadedBlock = JsonConvert.DeserializeObject<LoadedBlock>(File.ReadAllText(Path.GetFullPath(dataFile)));
                    Block block = new(loadedBlock);
                    BlockList.Add(block.ItemId, block);
                }
            }
        }

        public static Block Get(string name)
        {
            try
            {
                return BlockList[name];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }

    public class LoadedBlock
    {
        public string id;
        public string name;
        public int max_stack;
        public float hardness;
        public bool transparent;
        public string[] texture;
        public bool camera_relative;
        public bool gravity = false;
    }
}