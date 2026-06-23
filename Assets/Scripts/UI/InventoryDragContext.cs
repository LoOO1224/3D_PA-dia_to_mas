using DiaToMas.Data;

namespace DiaToMas.UI
{
    public static class InventoryDragContext
    {
        public static ShopItemData ItemData { get; private set; }
        public static bool IsHandled { get; private set; }

        public static void Begin(ShopItemData itemData)
        {
            ItemData = itemData;
            IsHandled = false;
        }

        public static void MarkHandled()
        {
            IsHandled = true;
        }

        public static void Clear()
        {
            ItemData = null;
            IsHandled = false;
        }
    }
}
