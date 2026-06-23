using System;
using DiaToMas.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DiaToMas.UI
{
    public class ShopItemButtonView : MonoBehaviour
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _priceText;
        [SerializeField] private Button _buyButton;

        private ShopItemData _itemData;
        private Action<ShopItemData> _onBuy;

        private void Awake()
        {
            _buyButton.onClick.AddListener(Buy);
        }

        public void Setup(ShopItemData itemData, string currencyName, Action<ShopItemData> onBuy)
        {
            _itemData = itemData;
            _onBuy = onBuy;

            _nameText.text = itemData.displayName;
            _descriptionText.text = itemData.description;
            _priceText.text = $"{itemData.priceAmount} {currencyName}";
        }

        private void Buy()
        {
            _onBuy?.Invoke(_itemData);
        }
    }
}
