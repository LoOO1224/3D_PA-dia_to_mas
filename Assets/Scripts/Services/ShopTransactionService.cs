using DiaToMas.Data;
using DiaToMas.Models;

namespace DiaToMas.Services
{
    public class ShopTransactionService
    {
        public bool TryBuy(PlayerModel playerModel, ShopStockModel stockModel, ShopItemData itemData, int amount, out string message)
        {
            if (playerModel == null || stockModel == null || itemData == null)
            {
                message = "거래 정보를 확인할 수 없습니다.";
                return false;
            }

            if (amount <= 0)
            {
                message = "구매 수량이 올바르지 않습니다.";
                return false;
            }

            if (!stockModel.CanTake(itemData.id, amount))
            {
                message = $"{itemData.displayName} 재고가 부족합니다.";
                return false;
            }

            int totalPrice = itemData.priceAmount * amount;
            if (!playerModel.WalletModel.TrySpend(itemData.priceCurrencyId, totalPrice))
            {
                message = $"{itemData.displayName} 구매에 필요한 재화가 부족합니다.";
                return false;
            }

            stockModel.TryTake(itemData.id, amount);
            playerModel.InventoryModel.AddItem(itemData.id, amount);
            message = $"{itemData.displayName} {amount}개 구매 완료";
            return true;
        }

        public bool TrySell(PlayerModel playerModel, ShopStockModel stockModel, ShopItemData itemData, int amount, out string message)
        {
            if (playerModel == null || stockModel == null || itemData == null)
            {
                message = "거래 정보를 확인할 수 없습니다.";
                return false;
            }

            if (amount <= 0)
            {
                message = "판매 수량이 올바르지 않습니다.";
                return false;
            }

            if (!playerModel.InventoryModel.TryRemoveItem(itemData.id, amount))
            {
                message = $"{itemData.displayName} 보유 수량이 부족합니다.";
                return false;
            }

            string sellCurrencyId = GetSellCurrencyId(itemData);
            int sellAmount = GetSellAmount(itemData) * amount;
            playerModel.WalletModel.AddAmount(sellCurrencyId, sellAmount);
            stockModel.AddStock(itemData.id, amount);
            message = $"{itemData.displayName} {amount}개 판매 완료";
            return true;
        }

        public bool TryDismantle(PlayerModel playerModel, ShopItemData itemData, int amount, out string message)
        {
            if (playerModel == null || itemData == null)
            {
                message = "분해 정보를 확인할 수 없습니다.";
                return false;
            }

            if (amount <= 0)
            {
                message = "분해 수량이 올바르지 않습니다.";
                return false;
            }

            if (!playerModel.InventoryModel.TryRemoveItem(itemData.id, amount))
            {
                message = $"{itemData.displayName} 보유 수량이 부족합니다.";
                return false;
            }

            string dismantleCurrencyId = string.IsNullOrWhiteSpace(itemData.dismantleCurrencyId)
                ? "crystal"
                : itemData.dismantleCurrencyId;
            int dismantleAmount = GetDismantleAmount(itemData) * amount;
            playerModel.WalletModel.AddAmount(dismantleCurrencyId, dismantleAmount);
            message = $"{itemData.displayName} {amount}개 분해 완료";
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

        private static int GetDismantleAmount(ShopItemData itemData)
        {
            return itemData.dismantleAmount > 0
                ? itemData.dismantleAmount
                : 1;
        }
    }
}
