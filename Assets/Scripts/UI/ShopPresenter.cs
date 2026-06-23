using System.Collections.Generic;
using DiaToMas.Data;
using DiaToMas.Managers;
using DiaToMas.Models;
using UnityEngine;
using UnityEngine.UI;

namespace DiaToMas.UI
{
    public class ShopPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _rootObject;
        [SerializeField] private Transform _itemRoot;
        [SerializeField] private Transform _inventoryRoot;
        [SerializeField] private ShopItemButtonView _itemButtonPrefab;
        [SerializeField] private InventoryItemRowView _inventoryItemRowPrefab;
        [SerializeField] private Text _walletText;
        [SerializeField] private Text _inventoryText;
        [SerializeField] private Text _feedbackText;
        [SerializeField] private Text _promptText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _sellLootButton;

        private readonly List<ShopItemButtonView> _spawnedItemRows = new();
        private readonly List<InventoryItemRowView> _spawnedInventoryRows = new();

        private bool IsOpen => _rootObject != null && _rootObject.activeSelf;

        private void Awake()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }

            if (_sellLootButton != null)
            {
                _sellLootButton.onClick.AddListener(SellLoot);
            }

            Close();
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        public void Open()
        {
            if (_rootObject == null)
            {
                return;
            }

            _rootObject.SetActive(true);
            GameManager.Inst?.SetPlayerInputLocked(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            HidePrompt();
            Refresh();
        }

        public void Close()
        {
            if (_rootObject != null)
            {
                _rootObject.SetActive(false);
            }

            GameManager.Inst?.SetPlayerInputLocked(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetFeedback(string.Empty);
        }

        public void ShowPrompt(string message)
        {
            if (_promptText == null || IsOpen)
            {
                return;
            }

            _promptText.gameObject.SetActive(true);
            _promptText.text = message;
        }

        public void HidePrompt()
        {
            if (_promptText != null)
            {
                _promptText.gameObject.SetActive(false);
            }
        }

        private void Refresh()
        {
            if (GameManager.Inst == null)
            {
                return;
            }

            ClearRows();
            RefreshWalletText();
            RefreshInventoryText();
            CreateItemRows();
            CreateInventoryRows();
        }

        private void CreateItemRows()
        {
            GameDataManager dataManager = GameManager.Inst.GameDataManager;
            ShopStockModel stockModel = GameManager.Inst.ShopStockModel;

            foreach (ShopItemData itemData in dataManager.ShopItemDataById.Values)
            {
                if (!itemData.isShopListed)
                {
                    continue;
                }

                ShopItemButtonView row = Instantiate(_itemButtonPrefab, _itemRoot);
                string currencyName = GetCurrencyName(itemData.priceCurrencyId);
                int stockCount = stockModel.GetStockCount(itemData.id);

                row.Setup(itemData, currencyName, stockCount, Buy);
                _spawnedItemRows.Add(row);
            }
        }

        private void CreateInventoryRows()
        {
            PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerModel.InventoryModel;

            foreach (InventoryItemModel itemModel in inventoryModel.ItemById.Values)
            {
                if (!GameManager.Inst.GameDataManager.TryGetShopItemData(itemModel.ItemId, out ShopItemData itemData))
                {
                    continue;
                }

                InventoryItemRowView row = Instantiate(_inventoryItemRowPrefab, _inventoryRoot);
                string sellCurrencyId = GetSellCurrencyId(itemData);
                string currencyName = GetCurrencyName(sellCurrencyId);
                int sellAmount = GetSellAmount(itemData);

                row.Setup(itemData, itemModel.Amount, currencyName, sellAmount, SellItem);
                _spawnedInventoryRows.Add(row);
            }
        }

        private void Buy(ShopItemData itemData)
        {
            PlayerModel playerModel = GameManager.Inst.PlayerModel;
            ShopStockModel stockModel = GameManager.Inst.ShopStockModel;
            bool isSuccess = GameManager.Inst.ShopTransactionService.TryBuy(playerModel, stockModel, itemData, out string message);
            SetFeedback(message);

            if (isSuccess)
            {
                Refresh();
            }
        }

        private void SellItem(ShopItemData itemData)
        {
            PlayerModel playerModel = GameManager.Inst.PlayerModel;
            ShopStockModel stockModel = GameManager.Inst.ShopStockModel;
            bool isSuccess = GameManager.Inst.ShopTransactionService.TrySell(playerModel, stockModel, itemData, out string message);
            SetFeedback(message);

            if (isSuccess)
            {
                Refresh();
            }
        }

        private void SellLoot()
        {
            PlayerModel playerModel = GameManager.Inst.PlayerModel;
            bool isSuccess = GameManager.Inst.ShopTransactionService.TrySellLoot(playerModel, out string message);
            SetFeedback(message);

            if (isSuccess)
            {
                Refresh();
            }
        }

        private void RefreshWalletText()
        {
            PlayerModel playerModel = GameManager.Inst.PlayerModel;
            GameDataManager dataManager = GameManager.Inst.GameDataManager;
            List<string> parts = new();

            foreach (CurrencyData currencyData in dataManager.CurrencyDataById.Values)
            {
                int amount = playerModel.WalletModel.GetAmount(currencyData.id);
                parts.Add($"{currencyData.displayName}: {amount}");
            }

            _walletText.text = string.Join("   ", parts);
        }

        private void RefreshInventoryText()
        {
            PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerModel.InventoryModel;
            _inventoryText.text = inventoryModel.ItemById.Count > 0
                ? "Inventory"
                : "Inventory: Empty";
        }

        private void SetFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message;
            }
        }

        private void ClearRows()
        {
            foreach (ShopItemButtonView row in _spawnedItemRows)
            {
                Destroy(row.gameObject);
            }

            foreach (InventoryItemRowView row in _spawnedInventoryRows)
            {
                Destroy(row.gameObject);
            }

            _spawnedItemRows.Clear();
            _spawnedInventoryRows.Clear();
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

        private static string GetCurrencyName(string currencyId)
        {
            return GameManager.Inst.GameDataManager.CurrencyDataById.TryGetValue(currencyId, out CurrencyData currencyData)
                ? currencyData.displayName
                : currencyId;
        }
    }
}
