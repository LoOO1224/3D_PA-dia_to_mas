using System;
using System.Collections.Generic;

namespace DiaToMas.Data
{
    [Serializable]
    public class GameDataList<T> where T : GameDataBase
    {
        public List<T> items = new();
    }
}
