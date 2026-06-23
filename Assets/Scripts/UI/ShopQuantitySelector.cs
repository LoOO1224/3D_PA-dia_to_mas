using DiaToMas.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace DiaToMas.UI
{
    public class ShopQuantitySelector : MonoBehaviour
    {
        [SerializeField] private Text _quantityText;
        [SerializeField] private int _minQuantity = 1;
        [SerializeField] private int _maxQuantity = 20;

        private int _quantity = 1;

        public int Quantity => _quantity;

        private void Awake()
        {
            RefreshView();
        }

        private void Update()
        {
            if (GameManager.Inst == null || !GameManager.Inst.IsPlayerInputLocked)
            {
                return;
            }

            if (!Input.GetKey(KeyCode.LeftControl))
            {
                return;
            }

            float scrollValue = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollValue) <= 0.001f)
            {
                return;
            }

            int delta = scrollValue > 0f ? 1 : -1;
            SetQuantity(_quantity + delta);
        }

        public void ResetQuantity()
        {
            SetQuantity(_minQuantity);
        }

        private void SetQuantity(int quantity)
        {
            _quantity = Mathf.Clamp(quantity, _minQuantity, _maxQuantity);
            RefreshView();
        }

        private void RefreshView()
        {
            if (_quantityText != null)
            {
                _quantityText.text = $"수량: {_quantity}";
            }
        }
    }
}
