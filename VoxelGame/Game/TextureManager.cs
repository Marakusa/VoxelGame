using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace VoxelGame.Game
{
    public class TextureManager
    {
        public static TextureManager Instance;

        private const int TileSize = 16;
        private const int AtlasSize = 16;
        private const int Dilation = 2;
        private Image<Rgba32> _textureAtlas;
        private Dictionary<string, float[]> _textures = new();

        public TextureManager()
        {
            Instance = this;
            GenerateTextureAtlas();
        }

        private void NewAtlas()
        {
            int paddedTile = TileSize + Dilation * 2;
            _textureAtlas = new Image<Rgba32>(AtlasSize * paddedTile, AtlasSize * paddedTile);
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
            NewAtlas();

            int indexX = 0;
            int indexY = 0;

            foreach (var textureFile in Directory.GetFiles("assets/textures/blocks/"))
            {
                if (Path.GetExtension(textureFile) == ".png")
                {
                    Image<Rgba32> image = Image.Load<Rgba32>(textureFile);

                    image.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < TileSize; y++)
                        {
                            var row = accessor.GetRowSpan(y);
                            for (int x = 0; x < TileSize; x++)
                            {
                                int atlasX = x + indexX * (TileSize + 2 * Dilation) + Dilation;
                                int atlasY = y + indexY * (TileSize + 2 * Dilation) + Dilation;
                                _textureAtlas[atlasX, atlasY] = row[x];
                            }
                        }

                        // Dilation (edge duplication)
                        for (int x = 0; x < TileSize; x++)
                        {
                            int atlasX = x + indexX * (TileSize + 2 * Dilation) + Dilation;
                            // Top
                            for (int i = 0; i < Dilation; i++)
                                _textureAtlas[atlasX, indexY * (TileSize + 2 * Dilation) + i] = _textureAtlas[atlasX, Dilation + indexY * (TileSize + 2 * Dilation)];
                            // Bottom
                            for (int i = 0; i < Dilation; i++)
                                _textureAtlas[atlasX, indexY * (TileSize + 2 * Dilation) + Dilation + TileSize + i] = _textureAtlas[atlasX, indexY * (TileSize + 2 * Dilation) + Dilation + TileSize - 1];
                        }

                        for (int y = 0; y < TileSize; y++)
                        {
                            int atlasY = y + indexY * (TileSize + 2 * Dilation) + Dilation;
                            // Left
                            for (int i = 0; i < Dilation; i++)
                                _textureAtlas[indexX * (TileSize + 2 * Dilation) + i, atlasY] = _textureAtlas[Dilation + indexX * (TileSize + 2 * Dilation), atlasY];
                            // Right
                            for (int i = 0; i < Dilation; i++)
                                _textureAtlas[indexX * (TileSize + 2 * Dilation) + Dilation + TileSize + i, atlasY] = _textureAtlas[indexX * (TileSize + 2 * Dilation) + Dilation + TileSize - 1, atlasY];
                        }

                        // Corners
                        for (int i = 0; i < Dilation; i++)
                        {
                            for (int j = 0; j < Dilation; j++)
                            {
                                // TL
                                _textureAtlas[indexX * (TileSize + 2 * Dilation) + i, indexY * (TileSize + 2 * Dilation) + j] =
                                    _textureAtlas[Dilation + indexX * (TileSize + 2 * Dilation), Dilation + indexY * (TileSize + 2 * Dilation)];
                                // TR
                                _textureAtlas[indexX * (TileSize + 2 * Dilation) + Dilation + TileSize + i, indexY * (TileSize + 2 * Dilation) + j] =
                                    _textureAtlas[indexX * (TileSize + 2 * Dilation) + Dilation + TileSize - 1, Dilation + indexY * (TileSize + 2 * Dilation)];
                                // BL
                                _textureAtlas[indexX * (TileSize + 2 * Dilation) + i, indexY * (TileSize + 2 * Dilation) + Dilation + TileSize + j] =
                                    _textureAtlas[Dilation + indexX * (TileSize + 2 * Dilation), Dilation + indexY * (TileSize + 2 * Dilation) + TileSize - 1];
                                // BR
                                _textureAtlas[indexX * (TileSize + 2 * Dilation) + Dilation + TileSize + i, indexY * (TileSize + 2 * Dilation) + Dilation + TileSize + j] =
                                    _textureAtlas[Dilation + indexX * (TileSize + 2 * Dilation) + TileSize - 1, Dilation + indexY * (TileSize + 2 * Dilation) + TileSize - 1];
                            }
                        }
                    });

                    int paddedSize = TileSize + 2 * Dilation;
                    float atlasSize = AtlasSize * paddedSize;

                    float u = (indexX * paddedSize + Dilation) / atlasSize;
                    float v = (indexY * paddedSize + Dilation) / atlasSize;
                    float size = TileSize / atlasSize;

                    _textures.Add(Path.GetFileNameWithoutExtension(textureFile), [u, v, size, size]);

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