namespace VoxelGame.Game
{
    public class InventorySlot
    {
        public readonly int id;
        public Item item;
        public int amount;

        public InventorySlot(int slotId)
        {
            id = slotId;
            item = null;
            amount = 0;
        }
    }
}
