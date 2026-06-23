using System.Collections.Generic;
using DiaToMas.Data;
using UnityEngine;

namespace DiaToMas.Managers
{
    public class GameDataManager : MonoBehaviour
    {
        [SerializeField] private TextAsset _currencyDataJson;
        [SerializeField] private TextAsset _shopItemDataJson;
        [SerializeField] private TextAsset _startingInventoryDataJson;
        [SerializeField] private TextAsset _playerMovementDataJson;

        private readonly Dictionary<string, CurrencyData> _currencyDataById = new();
        private readonly Dictionary<string, ShopItemData> _shopItemDataById = new();
        private readonly List<StartingInventoryData> _startingInventoryDataList = new();
        private PlayerMovementData _playerMovementData;

        public IReadOnlyDictionary<string, CurrencyData> CurrencyDataById => _currencyDataById;
        public IReadOnlyDictionary<string, ShopItemData> ShopItemDataById => _shopItemDataById;
        public IReadOnlyList<StartingInventoryData> StartingInventoryDataList => _startingInventoryDataList;
        public PlayerMovementData PlayerMovementData => _playerMovementData;

        public void LoadData()
        {
            LoadDictionary(_currencyDataJson, _currencyDataById);
            LoadDictionary(_shopItemDataJson, _shopItemDataById);
            LoadList(_startingInventoryDataJson, _startingInventoryDataList);
            _playerMovementData = LoadFirst<PlayerMovementData>(_playerMovementDataJson);
        }

        public bool TryGetShopItemData(string itemId, out ShopItemData itemData)
        {
            return _shopItemDataById.TryGetValue(itemId, out itemData);
        }

        private static void LoadDictionary<T>(TextAsset jsonAsset, Dictionary<string, T> target) where T : GameDataBase
        {
            target.Clear();

            if (jsonAsset == null)
            {
                return;
            }

            GameDataList<T> dataList = JsonUtility.FromJson<GameDataList<T>>(jsonAsset.text);
            foreach (T data in dataList.items)
            {
                target[data.id] = data;
            }
        }

        private static void LoadList<T>(TextAsset jsonAsset, List<T> target) where T : GameDataBase
        {
            target.Clear();

            if (jsonAsset == null)
            {
                return;
            }

            GameDataList<T> dataList = JsonUtility.FromJson<GameDataList<T>>(jsonAsset.text);
            target.AddRange(dataList.items);
        }

        private static T LoadFirst<T>(TextAsset jsonAsset) where T : GameDataBase
        {
            if (jsonAsset == null)
            {
                return null;
            }

            GameDataList<T> dataList = JsonUtility.FromJson<GameDataList<T>>(jsonAsset.text);
            return dataList.items.Count > 0 ? dataList.items[0] : null;
        }
    }
}
