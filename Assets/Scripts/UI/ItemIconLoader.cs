using DiaToMas.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DiaToMas.UI
{
    public static class ItemIconLoader
    {
        private const string DefaultIconResourcePath = "UI/GuiProCasualGame/Icon/Icon_Chest";

        public static void Apply(Image iconImage, ShopItemData itemData)
        {
            if (iconImage == null)
            {
                return;
            }

            string iconPath = string.IsNullOrWhiteSpace(itemData.iconResourcePath)
                ? DefaultIconResourcePath
                : itemData.iconResourcePath;
            Sprite iconSprite = Resources.Load<Sprite>(iconPath);
            iconImage.sprite = iconSprite;
            iconImage.enabled = iconSprite != null;
        }
    }
}
