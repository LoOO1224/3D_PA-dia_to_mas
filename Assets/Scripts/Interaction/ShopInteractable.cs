using DiaToMas.Player;
using DiaToMas.UI;
using UnityEngine;

namespace DiaToMas.Interaction
{
    public class ShopInteractable : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField] private ShopPresenter _shopPresenter;
        [SerializeField] private string _promptMessage = "상인과 거래: E 또는 좌클릭";

        private PlayerInputReader _currentInputReader;

        public string PromptMessage => _promptMessage;

        private void Awake()
        {
            _shopPresenter = _shopPresenter != null ? _shopPresenter : FindFirstObjectByType<ShopPresenter>();
        }

        private void Update()
        {
            TryOpenShop();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerInputReader inputReader))
            {
                _currentInputReader = inputReader;
                _shopPresenter?.ShowPrompt(_promptMessage);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_currentInputReader != null && other.gameObject == _currentInputReader.gameObject)
            {
                _currentInputReader = null;
                _shopPresenter?.HidePrompt();
            }
        }

        public void Interact(GameObject interactor)
        {
            _shopPresenter?.Open();
        }

        private void TryOpenShop()
        {
            if (_currentInputReader == null || !_currentInputReader.ConsumeInteract())
            {
                return;
            }

            Interact(_currentInputReader.gameObject);
        }
    }
}
