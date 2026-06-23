using DiaToMas.Data;
using DiaToMas.Models;
using DiaToMas.Services;
using UnityEngine;

namespace DiaToMas.Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameDataManager _gameDataManager;

        private PlayerModel _playerModel;
        private ShopStockModel _shopStockModel;
        private ShopTransactionService _shopTransactionService;
        private bool _isPlayerInputLocked;

        public static GameManager Inst { get; private set; }
        public GameDataManager GameDataManager => _gameDataManager;
        public PlayerModel PlayerModel => _playerModel;
        public ShopStockModel ShopStockModel => _shopStockModel;
        public ShopTransactionService ShopTransactionService => _shopTransactionService;
        public bool IsPlayerInputLocked => _isPlayerInputLocked;

        private void Awake()
        {
            if (Inst != null && Inst != this)
            {
                Destroy(gameObject);
                return;
            }

            Inst = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            _gameDataManager = _gameDataManager != null ? _gameDataManager : GetComponent<GameDataManager>();
            _gameDataManager.LoadData();

            _playerModel = new PlayerModel();
            foreach (CurrencyData currencyData in _gameDataManager.CurrencyDataById.Values)
            {
                _playerModel.WalletModel.SetAmount(currencyData.id, currencyData.startAmount);
            }

            foreach (StartingInventoryData inventoryData in _gameDataManager.StartingInventoryDataList)
            {
                _playerModel.InventoryModel.AddItem(inventoryData.itemId, inventoryData.amount);
            }

            _shopStockModel = new ShopStockModel();
            _shopStockModel.Initialize(_gameDataManager.ShopItemDataById.Values);
            _shopTransactionService = new ShopTransactionService();
        }

        public void SetPlayerInputLocked(bool isLocked)
        {
            _isPlayerInputLocked = isLocked;
        }
    }
}
