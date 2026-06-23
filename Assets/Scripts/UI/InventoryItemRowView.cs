using System;
using DiaToMas.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DiaToMas.UI
{
    public class InventoryItemRowView : MonoBehaviour
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _amountText;
        [SerializeField] private Text _sellPriceText;
        [SerializeField] private Button _sellButton;

        private ShopItemData _itemData;
        private Action<ShopItemData> _onSell;

        private void Awake()
        {
            _sellButton.onClick.AddListener(Sell);
        }

        public void Setup(ShopItemData itemData, int amount, string currencyName, int sellAmount, Action<ShopItemData> onSell)
        {
            _itemData = itemData;
            _onSell = onSell;

            _nameText.text = itemData.displayName;
            _amountText.text = $"x{amount}";
            _sellPriceText.text = $"{sellAmount} {currencyName}";
            _sellButton.interactable = amount > 0;
        }

        private void Sell()
        {
            _onSell?.Invoke(_itemData);
        }
    }
}
