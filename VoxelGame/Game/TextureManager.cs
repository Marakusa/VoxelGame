using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace VoxelGame.Game
{
    public class TextureManager
    {
        public static TextureManager Instance;

        private const int AtlasSize = 16;
        private Image<Rgba32> _textureAtlas = new(16 * AtlasSize, 16 * AtlasSize);
        private Dictionary<string, float[]> _textures = new();

        public TextureManager()
        {
            Instance = this;
            GenerateTextureAtlas();
        }

        public static UVTransform GetTexture(string name)
        {
            if (Instance._textures.ContainsKey(name))
            {
                var texture = Instance._textures[name];
                return new(texture[0], texture[1], texture[2], texture[3]);
            }
            
            return new(0, 0, 1, 1);
        }

        private void GenerateTextureAtlas()
        {
            _textures = new();
            _textureAtlas = new(16 * AtlasSize, 16 * AtlasSize);

            int indexX = 0;
            int indexY = 0;

            foreach (var textureFile in Directory.GetFiles("assets/textures/blocks/"))
            {
                if (Path.GetExtension(textureFile) == ".png")
                {
                    Image<Rgba32> image = Image.Load<Rgba32>(textureFile);

                    for (int y = 0; y < image.Height; y++)
                    {
                        var row = image.GetPixelRowSpan(y);

                        for (int x = 0; x < image.Width; x++)
                        {
                            byte r = row[x].R;
                            byte g = row[x].G;
                            byte b = row[x].B;
                            byte a = row[x].A;
                            _textureAtlas[x + indexX * 16, y + indexY * 16] = new Rgba32(r, g, b, a);
                        }
                    }

                    const float divider = 1f / 16.0f;
                    _textures.Add(Path.GetFileNameWithoutExtension(textureFile), new[]
                    {
                        indexX * divider,
                        indexY * divider,
                        divider,
                        divider
                    });

                    indexX++;

                    if (indexX > AtlasSize)
                    {
                        indexX = 0;
                        indexY++;
                    }
                }
            }

            _textureAtlas.Save("assets/atlas.png");
        }
    }

    public class UVTransform
    {
        public float UvX;
        public float UvY;
        public float UvW;
        public float UvH;

        public UVTransform(float x, float y, float w, float h)
        {
            UvX = x;
            UvY = y;
            UvW = w;
            UvH = h;
        }
    }
}