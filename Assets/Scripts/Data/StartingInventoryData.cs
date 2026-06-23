using System;

namespace DiaToMas.Data
{
    [Serializable]
    public class StartingInventoryData : GameDataBase
    {
        public string itemId;
        public int amount;
    }
}
