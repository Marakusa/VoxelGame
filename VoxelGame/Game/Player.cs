namespace VoxelGame.Game
{
    public class Player
    {
        private InventorySlot[] _inventorySlots;
        private const int InventoryRows = 4, InventoryColumns = 9;
        private int _activeSlot;
        
        public Player()
        {
            _inventorySlots = new InventorySlot[InventoryRows * InventoryColumns];

            for (int i = 0; i < _inventorySlots.Length; i++) _inventorySlots[i] = new(i);
            
            _activeSlot = 0;

            AddItem(Blocks.Get("dirt"));
            AddItem(Blocks.Get("grass_block"));
            AddItem(Blocks.Get("stone"));
            AddItem(Blocks.Get("sand"));
            AddItem(Blocks.Get("debug"));
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
