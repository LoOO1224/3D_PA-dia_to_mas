using System.Collections.Generic;

namespace DiaToMas.Models
{
    public class PlayerInventoryModel
    {
        private readonly Dictionary<string, InventoryItemModel> _itemById = new();

        public IReadOnlyDictionary<string, InventoryItemModel> ItemById => _itemById;

        public void AddItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return;
            }

            if (_itemById.TryGetValue(itemId, out InventoryItemModel itemModel))
            {
                itemModel.AddAmount(amount);
                return;
            }

            _itemById.Add(itemId, new InventoryItemModel(itemId, amount));
        }

        public bool TryRemoveItem(string itemId, int amount)
        {
            if (!_itemById.TryGetValue(itemId, out InventoryItemModel itemModel))
            {
                return false;
            }

            if (!itemModel.TryRemoveAmount(amount))
            {
                return false;
            }

            if (itemModel.Amount <= 0)
            {
                _itemById.Remove(itemId);
            }

            return true;
        }

        public int GetAmount(string itemId)
        {
            return _itemById.TryGetValue(itemId, out InventoryItemModel itemModel) ? itemModel.Amount : 0;
        }
    }
}
