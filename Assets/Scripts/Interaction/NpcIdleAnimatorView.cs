using UnityEngine;

namespace DiaToMas.Interaction
{
    public class NpcIdleAnimatorView : MonoBehaviour
    {
        private const int BaseLayerIndex = 0;

        [SerializeField] private Animator _animator;
        [SerializeField] private string _baseLayerName = "Base Layer";
        [SerializeField] private string _idleStateName = "Mage_Idle";

        private void Awake()
        {
            _animator = _animator != null ? _animator : GetComponentInChildren<Animator>();
            PlayIdle();
        }

        private void OnEnable()
        {
            PlayIdle();
        }

        public void PlayIdle()
        {
            if (_animator == null || string.IsNullOrWhiteSpace(_idleStateName))
            {
                return;
            }

            int stateHash = Animator.StringToHash($"{_baseLayerName}.{_idleStateName}");
            if (_animator.HasState(BaseLayerIndex, stateHash))
            {
                _animator.Play(stateHash, BaseLayerIndex, 0f);
            }
        }
    }
}
