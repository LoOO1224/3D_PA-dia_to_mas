using DiaToMas.Data;
using DiaToMas.Models;

namespace DiaToMas.Services
{
    public class ShopTransactionService
    {
        public bool TryBuy(PlayerModel playerModel, ShopStockModel stockModel, ShopItemData itemData, out string message)
        {
            if (playerModel == null || stockModel == null || itemData == null)
            {
                message = "Transaction data is missing.";
                return false;
            }

            if (!stockModel.CanTake(itemData.id, 1))
            {
                message = $"{itemData.displayName} is out of stock.";
                return false;
            }

            if (!playerModel.WalletModel.TrySpend(itemData.priceCurrencyId, itemData.priceAmount))
            {
                message = $"Not enough {itemData.priceCurrencyId}.";
                return false;
            }

            stockModel.TryTake(itemData.id, 1);
            playerModel.InventoryModel.AddItem(itemData.id, 1);
            message = $"Bought {itemData.displayName}.";
            return true;
        }

        public bool TrySell(PlayerModel playerModel, ShopStockModel stockModel, ShopItemData itemData, out string message)
        {
            if (playerModel == null || stockModel == null || itemData == null)
            {
                message = "Transaction data is missing.";
                return false;
            }

            if (!playerModel.InventoryModel.TryRemoveItem(itemData.id, 1))
            {
                message = $"You do not have {itemData.displayName}.";
                return false;
            }

            string sellCurrencyId = GetSellCurrencyId(itemData);
            int sellAmount = GetSellAmount(itemData);
            playerModel.WalletModel.AddAmount(sellCurrencyId, sellAmount);
            stockModel.AddStock(itemData.id, 1);
            message = $"Sold {itemData.displayName} for {sellAmount} {sellCurrencyId}.";
            return true;
        }

        public bool TrySellLoot(PlayerModel playerModel, out string message)
        {
            const string lootCurrencyId = "loot";
            const string goldCurrencyId = "gold";
            const int goldPerLoot = 25;

            if (playerModel == null || !playerModel.WalletModel.TrySpend(lootCurrencyId, 1))
            {
                message = "You do not have loot to sell.";
                return false;
            }

            playerModel.WalletModel.AddAmount(goldCurrencyId, goldPerLoot);
            message = $"Sold loot for {goldPerLoot} gold.";
            return true;
        }

        private static string GetSellCurrencyId(ShopItemData itemData)
        {
            return string.IsNullOrWhiteSpace(itemData.sellCurrencyId)
                ? itemData.priceCurrencyId
                : itemData.sellCurrencyId;
        }

        private static int GetSellAmount(ShopItemData itemData)
        {
            return itemData.sellAmount > 0
                ? itemData.sellAmount
                : itemData.priceAmount / 2;
        }
    }
}
