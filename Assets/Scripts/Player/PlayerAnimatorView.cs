using System.Collections.Generic;
using UnityEngine;

namespace DiaToMas.Player
{
    public class PlayerAnimatorView : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");

        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerMovement _movement;

        private readonly HashSet<int> _parameterHashes = new();

        private void Awake()
        {
            _animator = _animator != null ? _animator : GetComponentInChildren<Animator>();
            _movement = _movement != null ? _movement : GetComponent<PlayerMovement>();
            CacheParameters();
        }

        private void Update()
        {
            UpdateAnimator();
        }

        private void CacheParameters()
        {
            _parameterHashes.Clear();

            if (_animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in _animator.parameters)
            {
                _parameterHashes.Add(parameter.nameHash);
            }
        }

        private void UpdateAnimator()
        {
            if (_animator == null || _movement == null)
            {
                return;
            }

            SetFloat(SpeedHash, _movement.MoveAmount);
            SetBool(IsGroundedHash, _movement.IsGrounded);
            SetFloat(VerticalSpeedHash, _movement.VerticalSpeed);
        }

        private void SetFloat(int parameterHash, float value)
        {
            if (_parameterHashes.Contains(parameterHash))
            {
                _animator.SetFloat(parameterHash, value);
            }
        }

        private void SetBool(int parameterHash, bool value)
        {
            if (_parameterHashes.Contains(parameterHash))
            {
                _animator.SetBool(parameterHash, value);
            }
        }
    }
}
