using System.Collections.Generic;

namespace DiaToMas.Models
{
    public class PlayerInventoryModel
    {
        private readonly Dictionary<string, InventoryItemModel> _itemById = new();

        public IReadOnlyDictionary<string, InventoryItemModel> ItemById => _itemById;

        public void AddItem(string itemId, int amount)
        {
            if (_itemById.TryGetValue(itemId, out InventoryItemModel itemModel))
            {
                itemModel.AddAmount(amount);
                return;
            }

            _itemById.Add(itemId, new InventoryItemModel(itemId, amount));
        }
    }
}
