namespace DiaToMas.Models
{
    public class InventoryItemModel
    {
        public InventoryItemModel(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }

        public string ItemId { get; }
        public int Amount { get; private set; }

        public void AddAmount(int amount)
        {
            Amount += amount;
        }

        public bool CanRemove(int amount)
        {
            return amount > 0 && Amount >= amount;
        }

        public bool TryRemoveAmount(int amount)
        {
            if (!CanRemove(amount))
            {
                return false;
            }

            Amount -= amount;
            return true;
        }
    }
}
