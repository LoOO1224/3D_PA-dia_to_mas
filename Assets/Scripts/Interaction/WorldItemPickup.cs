using DiaToMas.Managers;
using DiaToMas.Player;
using UnityEngine;

namespace DiaToMas.Interaction
{
    public class WorldItemPickup : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField] private string _itemId;
        [SerializeField] private int _amount = 1;

        private PlayerInputReader _currentInputReader;

        public string ItemId => _itemId;
        public int Amount => _amount;

        private void Update()
        {
            TryPickupFromInput();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerInputReader inputReader))
            {
                _currentInputReader = inputReader;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_currentInputReader != null && other.gameObject == _currentInputReader.gameObject)
            {
                _currentInputReader = null;
            }
        }

        public void Setup(string itemId, int amount)
        {
            _itemId = itemId;
            _amount = amount;
        }

        public void Interact(GameObject interactor)
        {
            if (string.IsNullOrWhiteSpace(_itemId) || _amount <= 0 || GameManager.Inst == null)
            {
                return;
            }

            GameManager.Inst.PlayerModel.InventoryModel.AddItem(_itemId, _amount);
            Destroy(gameObject);
        }

        private void TryPickupFromInput()
        {
            if (_currentInputReader == null || !_currentInputReader.ConsumeInteract())
            {
                return;
            }

            Interact(_currentInputReader.gameObject);
        }
    }
}
