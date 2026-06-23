using DiaToMas.Interaction;
using DiaToMas.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DiaToMas.Player
{
    public class PlayerClickInteractor : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _interactDistance = 6f;
        [SerializeField] private LayerMask _interactableLayerMask = ~0;

        private void Awake()
        {
            _camera = _camera != null ? _camera : Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (GameManager.Inst != null && GameManager.Inst.IsPlayerInputLocked)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            TryInteract();
        }

        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        private void TryInteract()
        {
            if (_camera == null)
            {
                return;
            }

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactableLayerMask, QueryTriggerInteraction.Collide))
            {
                return;
            }

            IPlayerInteractable interactable = hit.collider.GetComponentInParent<IPlayerInteractable>();
            interactable?.Interact(gameObject);
        }
    }
}
