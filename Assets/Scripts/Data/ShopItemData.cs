using System;

namespace DiaToMas.Data
{
    [Serializable]
    public class ShopItemData : GameDataBase
    {
        public string displayName;
        public string description;
        public string priceCurrencyId;
        public int priceAmount;
        public string sellCurrencyId;
        public int sellAmount;
        public bool isStackable;
        public int stockCount;
    }
}
