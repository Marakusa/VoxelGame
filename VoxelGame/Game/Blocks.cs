using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VoxelGame.Game
{
    public class Blocks
    {
        public static Blocks Instance;

        private readonly Dictionary<string, Block> _blocks = new();

        public Blocks()
        {
            Instance = this;
            _blocks = new();
            LoadBlocks();
        }

        private void LoadBlocks()
        {
            foreach (var dataFile in Directory.GetFiles("assets/data/blocks/"))
            {
                if (Path.GetExtension(dataFile) == ".json")
                {
                    LoadedBlock loadedBlock = JsonConvert.DeserializeObject<LoadedBlock>(File.ReadAllText(Path.GetFullPath(dataFile)));
                    Block block = new(loadedBlock);
                    _blocks.Add(block.BlockId, block);
                }
            }
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
                return null;
            }
        }
    }

    public class LoadedBlock
    {
        public string id;
        public string name;
        public int max_stack;
        public bool transparent;
        public string[] texture;
        public bool camera_relative;
        public bool gravity = false;
    }
}