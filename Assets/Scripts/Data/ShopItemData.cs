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
        public bool isStackable;
        public int stockCount;
    }
}
