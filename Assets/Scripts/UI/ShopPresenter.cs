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
        [SerializeField] private ShopItemButtonView _itemButtonPrefab;
        [SerializeField] private Text _walletText;
        [SerializeField] private Text _inventoryText;
        [SerializeField] private Text _feedbackText;
        [SerializeField] private Text _promptText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _sellLootButton;

        private readonly List<ShopItemButtonView> _spawnedRows = new();

        private void Awake()
        {
            _closeButton.onClick.AddListener(Close);
            _sellLootButton.onClick.AddListener(SellLoot);
            Close();
        }

        public void Open()
        {
            _rootObject.SetActive(true);
            HidePrompt();
            Refresh();
        }

        public void Close()
        {
            _rootObject.SetActive(false);
            SetFeedback(string.Empty);
        }

        public void ShowPrompt(string message)
        {
            if (_promptText == null)
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
            ClearRows();
            RefreshWalletText();
            RefreshInventoryText();
            CreateRows();
        }

        private void CreateRows()
        {
            GameDataManager dataManager = GameManager.Inst.GameDataManager;
            foreach (ShopItemData itemData in dataManager.ShopItemDataById.Values)
            {
                ShopItemButtonView row = Instantiate(_itemButtonPrefab, _itemRoot);
                string currencyName = dataManager.CurrencyDataById.TryGetValue(itemData.priceCurrencyId, out CurrencyData currencyData)
                    ? currencyData.displayName
                    : itemData.priceCurrencyId;

                row.Setup(itemData, currencyName, Buy);
                _spawnedRows.Add(row);
            }
        }

        private void Buy(ShopItemData itemData)
        {
            PlayerModel playerModel = GameManager.Inst.PlayerModel;
            bool isSuccess = GameManager.Inst.ShopTransactionService.TryBuy(playerModel, itemData, out string message);
            SetFeedback(message);

            if (isSuccess)
            {
                RefreshWalletText();
                RefreshInventoryText();
            }
        }

        private void SellLoot()
        {
            PlayerModel playerModel = GameManager.Inst.PlayerModel;
            bool isSuccess = GameManager.Inst.ShopTransactionService.TrySellLoot(playerModel, out string message);
            SetFeedback(message);

            if (isSuccess)
            {
                RefreshWalletText();
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
            List<string> parts = new();

            foreach (InventoryItemModel itemModel in inventoryModel.ItemById.Values)
            {
                string displayName = GameManager.Inst.GameDataManager.TryGetShopItemData(itemModel.ItemId, out ShopItemData itemData)
                    ? itemData.displayName
                    : itemModel.ItemId;

                parts.Add($"{displayName} x{itemModel.Amount}");
            }

            _inventoryText.text = parts.Count > 0 ? string.Join("   ", parts) : "Inventory: Empty";
        }

        private void SetFeedback(string message)
        {
            _feedbackText.text = message;
        }

        private void ClearRows()
        {
            foreach (ShopItemButtonView row in _spawnedRows)
            {
                Destroy(row.gameObject);
            }

            _spawnedRows.Clear();
        }
    }
}
