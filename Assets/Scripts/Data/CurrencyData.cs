using System;

namespace DiaToMas.Data
{
    [Serializable]
    public class CurrencyData : GameDataBase
    {
        public string displayName;
        public string iconText;
        public int startAmount;
    }
}
