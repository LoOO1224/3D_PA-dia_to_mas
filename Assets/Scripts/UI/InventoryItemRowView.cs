using System;
using DiaToMas.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DiaToMas.UI
{
    public class InventoryItemRowView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _amountText;
        [SerializeField] private Text _sellPriceText;
        [SerializeField] private Button _sellButton;
        [SerializeField] private Button _dismantleButton;

        private ShopItemData _itemData;
        private Action<ShopItemData> _onSell;
        private Action<ShopItemData> _onDismantle;
        private Action<ShopItemData> _onDropToWorld;
        private bool _isDragging;

        private void Awake()
        {
            _sellButton.onClick.AddListener(Sell);
            _dismantleButton.onClick.AddListener(Dismantle);
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
            _sellPriceText.text = $"{sellAmount} {currencyName}";
            _sellButton.interactable = amount > 0;
            _dismantleButton.interactable = amount > 0;
            SetActionButtonsVisible(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                SetActionButtonsVisible(!_sellButton.gameObject.activeSelf);
            }
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

        private void SetActionButtonsVisible(bool isVisible)
        {
            _sellButton.gameObject.SetActive(isVisible);
            _dismantleButton.gameObject.SetActive(isVisible);
        }
    }
}
