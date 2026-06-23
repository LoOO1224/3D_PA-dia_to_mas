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
        [SerializeField] private Text _stockText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _buyButton;

        private ShopItemData _itemData;
        private Action<ShopItemData> _onBuy;

        private void Awake()
        {
            _buyButton.onClick.AddListener(Buy);
        }

        public void Setup(ShopItemData itemData, string currencyName, int stockCount, Action<ShopItemData> onBuy)
        {
            _itemData = itemData;
            _onBuy = onBuy;

            _nameText.text = itemData.displayName;
            _descriptionText.text = itemData.description;
            _priceText.text = $"{itemData.priceAmount} {currencyName}";
            _stockText.text = stockCount > 0 ? $"재고 {stockCount}" : "품절";
            _buyButton.interactable = stockCount > 0;
            ItemIconLoader.Apply(_iconImage, itemData);
        }

        private void Buy()
        {
            _onBuy?.Invoke(_itemData);
        }
    }
}
