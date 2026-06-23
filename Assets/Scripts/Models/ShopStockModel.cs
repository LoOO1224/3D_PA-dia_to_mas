using System.Collections.Generic;
using DiaToMas.Data;

namespace DiaToMas.Models
{
    public class ShopStockModel
    {
        private readonly Dictionary<string, int> _stockCountByItemId = new();

        public IReadOnlyDictionary<string, int> StockCountByItemId => _stockCountByItemId;

        public void Initialize(IEnumerable<ShopItemData> itemDataList)
        {
            _stockCountByItemId.Clear();

            foreach (ShopItemData itemData in itemDataList)
            {
                _stockCountByItemId[itemData.id] = itemData.stockCount;
            }
        }

        public int GetStockCount(string itemId)
        {
            return _stockCountByItemId.TryGetValue(itemId, out int stockCount) ? stockCount : 0;
        }

        public bool CanTake(string itemId, int amount)
        {
            return amount > 0 && GetStockCount(itemId) >= amount;
        }

        public bool TryTake(string itemId, int amount)
        {
            if (!CanTake(itemId, amount))
            {
                return false;
            }

            _stockCountByItemId[itemId] = GetStockCount(itemId) - amount;
            return true;
        }

        public void AddStock(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return;
            }

            _stockCountByItemId[itemId] = GetStockCount(itemId) + amount;
        }
    }
}
