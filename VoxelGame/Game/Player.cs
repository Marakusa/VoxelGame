using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelGame.Engine;

namespace VoxelGame.Game
{
    public class Player
    {
        private readonly InventorySlot[] _inventorySlots;
        private const int InventoryRows = 4, InventoryColumns = 9;
        private int _activeSlot;
        
        private readonly Camera _camera;

        private HighlightBlock _highlightBlock;

        public Player(Camera playerCamera, HighlightBlock highlightBlock)
        {
            _inventorySlots = new InventorySlot[InventoryRows * InventoryColumns];

            for (int i = 0; i < _inventorySlots.Length; i++) _inventorySlots[i] = new(i);
            
            _activeSlot = 0;

            AddItem(Blocks.Get("dirt"));
            AddItem(Blocks.Get("grass_block"));
            AddItem(Blocks.Get("stone"));
            AddItem(Blocks.Get("sand"));
            AddItem(Blocks.Get("debug"));
            
            _camera = playerCamera;
            _highlightBlock = highlightBlock;
        }

        private Vector3 _hitPoint = Vector3.NegativeInfinity;
        private Vector3 _lastHitPoint = Vector3.NegativeInfinity;
        private Vector2 _chunkPoint = Vector2.NegativeInfinity;
        private Chunk _hitChunk;

        public void Update()
        {
            CheckBlockRaycast();
        }
        
        private void CheckBlockRaycast()
        {
            _hitPoint = Vector3.NegativeInfinity;
            _hitChunk = null;
            _chunkPoint = Vector2.NegativeInfinity;
            
            Ray ray = new(_camera.Position, _camera.Front);

            for (; ray.GetLength() <= 6f; ray.Step())
            {
                Vector3 rayBlockPosition = ray.GetEndPoint();
                int x = (int)Math.Floor(rayBlockPosition.X);
                int y = (int)Math.Floor(rayBlockPosition.Y);
                int z = (int)Math.Floor(rayBlockPosition.Z);

                Chunk chunk = ChunkManager.GetChunkByPoint(new(x, y, z));
                
                if (chunk != null && chunk.HasBlock((int)Math.Floor(x - chunk.Position.X), y, (int)Math.Floor(z - chunk.Position.Y)))
                {
                    x = (int)Math.Floor(x - chunk.Position.X);
                    z = (int)Math.Floor(z - chunk.Position.Y);
                    
                    _hitPoint = new(x, y, z);
                    _lastHitPoint = ray.GetLastPoint();
                    _hitChunk = chunk;
                    _chunkPoint = chunk.Position;
                    
                    break;
                }
            }
            
            _highlightBlock.Position = new Vector3((float)Math.Floor(_hitPoint.X) + _chunkPoint.X, (float)Math.Floor(_hitPoint.Y), (float)Math.Floor(_hitPoint.Z) + _chunkPoint.Y);
            _highlightBlock.Generate();
        }

        public void MouseDown(MouseButtonEventArgs e)
        {
            if (_hitChunk != null && _hitPoint != Vector3.NegativeInfinity)
            {
                if (e.Button == MouseButton.Left)
                {
                    int x = (int)Math.Floor(_hitPoint.X);
                    int y = (int)Math.Floor(_hitPoint.Y);
                    int z = (int)Math.Floor(_hitPoint.Z);
                    _hitChunk.DestroyBlock(x, y, z);

                    // TODO: Fix neighbor chunk faces on break
                    /*var neighbor = ChunkManager.GetChunkByPoint(new(x - 1, y, z));
                    if (neighbor != null && neighbor != _hitChunk)
                        neighbor.RegenerateMesh();
                    
                    neighbor = ChunkManager.GetChunkByPoint(new(x + 1, y, z));
                    if (neighbor != null && neighbor != _hitChunk)
                        neighbor.RegenerateMesh();
                    
                    neighbor = ChunkManager.GetChunkByPoint(new(x, y, z - 1));
                    if (neighbor != null && neighbor != _hitChunk)
                        neighbor.RegenerateMesh();
                    
                    neighbor = ChunkManager.GetChunkByPoint(new(x, y, z + 1));
                    if (neighbor != null && neighbor != _hitChunk)
                        neighbor.RegenerateMesh();*/
                }
                else if (e.Button == MouseButton.Right)
                {
                    // TODO: Fix blocks spawning opposite side if block placed next to another block at the borders of two chunks |^    #|_<--
                    int x = (int)Math.Floor(_lastHitPoint.X);
                    int y = (int)Math.Floor(_lastHitPoint.Y);
                    int z = (int)Math.Floor(_lastHitPoint.Z);

                    Chunk chunk = ChunkManager.GetChunkByPoint(new(x, y, z));

                    if (chunk != null)
                    {
                        int cx = (int)Math.Floor(x - chunk.Position.X);
                        int cz = (int)Math.Floor(z - chunk.Position.Y);

                        var item = GetHotbarItem().item;

                        if (item != null && item.GetType() == typeof(Block))
                            _hitChunk.PlaceBlock(cx, y, cz, (Block)item);
                    }
                }
            }
        }

        public void SetCurrentSlot(int slot)
        {
            _activeSlot = slot;
            
            if (_activeSlot < 0)
                _activeSlot = 8;

            if (_activeSlot >= 9)
                _activeSlot = 0;
        }
        public int GetCurrentSlot()
        {
            if (_activeSlot < 0)
                _activeSlot = 8;

            if (_activeSlot >= 9)
                _activeSlot = 0;

            return _activeSlot;
        }

        public InventorySlot GetHotbarItem()
        {
            var slot = _inventorySlots[GetCurrentSlot()];
            
            if (slot.item == null || slot.amount <= 0)
            {
                slot.item = null;
                slot.amount = 0;
            }
            
            return slot;
        }

        public InventorySlot GetSlot(int id)
        {
            var slot = _inventorySlots[id];

            if (slot.item == null || slot.amount <= 0)
            {
                slot.item = null;
                slot.amount = 0;
            }
            
            return slot;
        }

        public bool AddItem(Item item)
        {
            if (item.GetType() == typeof(Block) && ((Block)item).Holdable)
            {
                foreach (var slot in _inventorySlots)
                {
                    InventorySlot s = GetSlot(slot.id);
                    if (s.item == null)
                    {
                        s.item = item;
                        s.amount = 1;
                        return true;
                    }

                    if (s.item == item)
                    {
                        s.amount++;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
