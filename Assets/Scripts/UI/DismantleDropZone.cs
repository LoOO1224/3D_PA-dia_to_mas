using UnityEngine;
using UnityEngine.EventSystems;

namespace DiaToMas.UI
{
    public class DismantleDropZone : MonoBehaviour, IDropHandler
    {
        private ShopPresenter _shopPresenter;

        private void Awake()
        {
            _shopPresenter = GetComponentInParent<ShopPresenter>();
            _shopPresenter = _shopPresenter != null ? _shopPresenter : FindFirstObjectByType<ShopPresenter>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (InventoryDragContext.ItemData == null)
            {
                return;
            }

            InventoryDragContext.MarkHandled();
            _shopPresenter?.DismantleItem(InventoryDragContext.ItemData);
        }
    }
}
