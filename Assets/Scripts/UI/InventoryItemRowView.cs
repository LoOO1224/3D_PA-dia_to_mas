using System;
using DiaToMas.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DiaToMas.UI
{
    public class InventoryItemRowView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _amountText;
        [SerializeField] private Text _sellPriceText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _sellButton;
        [SerializeField] private Button _dismantleButton;

        private ShopItemData _itemData;
        private Action<ShopItemData> _onSell;
        private Action<ShopItemData> _onDismantle;
        private Action<ShopItemData> _onDropToWorld;
        private bool _isDragging;

        private void Awake()
        {
            if (_sellButton != null)
            {
                _sellButton.onClick.AddListener(Sell);
            }

            if (_dismantleButton != null)
            {
                _dismantleButton.onClick.AddListener(Dismantle);
            }
        }

        public void Setup(
            ShopItemData itemData,
            int amount,
            string currencyName,
            int sellAmount,
            Action<ShopItemData> onSell,
            Action<ShopItemData> onDismantle,
            Action<ShopItemData> onDropToWorld)
        {
            _itemData = itemData;
            _onSell = onSell;
            _onDismantle = onDismantle;
            _onDropToWorld = onDropToWorld;

            _nameText.text = itemData.displayName;
            _amountText.text = $"x{amount}";
            _sellPriceText.text = sellAmount > 0 ? $"{sellAmount} {currencyName}" : currencyName;
            SetButtonVisible(_sellButton, onSell != null);
            SetButtonVisible(_dismantleButton, onDismantle != null);
            SetButtonInteractable(_sellButton, amount > 0 && onSell != null);
            SetButtonInteractable(_dismantleButton, amount > 0 && onDismantle != null);
            ItemIconLoader.Apply(_iconImage, itemData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            InventoryDragContext.Begin(_itemData);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging && !InventoryDragContext.IsHandled)
            {
                _onDropToWorld?.Invoke(_itemData);
            }

            _isDragging = false;
            InventoryDragContext.Clear();
        }

        private void Sell()
        {
            _onSell?.Invoke(_itemData);
        }

        private void Dismantle()
        {
            _onDismantle?.Invoke(_itemData);
        }

        private static void SetButtonVisible(Button button, bool isVisible)
        {
            if (button != null)
            {
                button.gameObject.SetActive(isVisible);
            }
        }

        private static void SetButtonInteractable(Button button, bool isInteractable)
        {
            if (button != null)
            {
                button.interactable = isInteractable;
            }
        }
    }
}
