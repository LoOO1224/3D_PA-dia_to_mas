using System.Collections.Generic;
using DiaToMas.Data;
using DiaToMas.Managers;
using DiaToMas.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DiaToMas.UI
{
    public class PlayerInventoryPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _rootObject;
        [SerializeField] private Transform _inventoryRoot;
        [SerializeField] private InventoryItemRowView _inventoryItemRowPrefab;
        [SerializeField] private Text _walletText;
        [SerializeField] private Text _feedbackText;
        [SerializeField] private Button _closeButton;

        private readonly List<InventoryItemRowView> _spawnedInventoryRows = new();

        private bool IsOpen => _rootObject != null && _rootObject.activeSelf;

        private void Awake()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }

            Close();
        }

        private void Update()
        {
            if (IsOpen)
            {
                UpdateOpenInput();
                return;
            }

            if (CanOpen() && Input.GetKeyDown(KeyCode.I))
            {
                Open();
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
            Refresh();
        }

        public void Close()
        {
            if (_rootObject != null)
            {
                _rootObject.SetActive(false);
            }

            if (GameManager.Inst != null && GameManager.Inst.IsPlayerInputLocked)
            {
                GameManager.Inst.SetPlayerInputLocked(false);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetFeedback(string.Empty);
        }

        private void UpdateOpenInput()
        {
            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private bool CanOpen()
        {
            if (GameManager.Inst == null || GameManager.Inst.IsPlayerInputLocked)
            {
                return false;
            }

            return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
        }

        private void Refresh()
        {
            if (GameManager.Inst == null)
            {
                return;
            }

            ClearRows();
            RefreshWalletText();
            CreateInventoryRows();
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
                row.Setup(itemData, itemModel.Amount, "드래그해서 드롭", 0, null, null, DropItemToWorld);
                _spawnedInventoryRows.Add(row);
            }

            if (_spawnedInventoryRows.Count <= 0)
            {
                SetFeedback("인벤토리가 비어 있습니다.");
            }
        }

        private void DropItemToWorld(ShopItemData itemData)
        {
            if (GameManager.Inst == null || GameManager.Inst.WorldItemDropper == null)
            {
                SetFeedback("아이템을 바닥에 내려놓을 수 없습니다.");
                return;
            }

            bool isSuccess = GameManager.Inst.WorldItemDropper.TryDrop(itemData, 1, out string message);
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

        private void SetFeedback(string message)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message;
            }
        }

        private void ClearRows()
        {
            foreach (InventoryItemRowView row in _spawnedInventoryRows)
            {
                Destroy(row.gameObject);
            }

            _spawnedInventoryRows.Clear();
        }
    }
}
