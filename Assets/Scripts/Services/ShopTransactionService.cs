using DiaToMas.Data;
using DiaToMas.Models;

namespace DiaToMas.Services
{
    public class ShopTransactionService
    {
        public bool TryBuy(PlayerModel playerModel, ShopItemData itemData, out string message)
        {
            if (playerModel == null || itemData == null)
            {
                message = "구매 정보를 확인할 수 없습니다.";
                return false;
            }

            if (!playerModel.WalletModel.TrySpend(itemData.priceCurrencyId, itemData.priceAmount))
            {
                message = "재화가 부족합니다.";
                return false;
            }

            playerModel.InventoryModel.AddItem(itemData.id, 1);
            message = $"{itemData.displayName} 구매 완료";
            return true;
        }

        public bool TrySellLoot(PlayerModel playerModel, out string message)
        {
            const string lootCurrencyId = "loot";
            const string goldCurrencyId = "gold";
            const int goldPerLoot = 25;

            if (playerModel == null || !playerModel.WalletModel.TrySpend(lootCurrencyId, 1))
            {
                message = "판매할 전리품이 없습니다.";
                return false;
            }

            playerModel.WalletModel.AddAmount(goldCurrencyId, goldPerLoot);
            message = "전리품을 판매해 골드를 획득했습니다.";
            return true;
        }
    }
}
