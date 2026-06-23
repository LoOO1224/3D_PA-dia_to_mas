using System.Collections.Generic;
using UnityEngine;

namespace DiaToMas.Player
{
    public class PlayerAnimatorView : MonoBehaviour
    {
        private const int BaseLayerIndex = 0;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");

        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private string _baseLayerName = "Base Layer";
        [SerializeField] private string _idleStateName = "Mage_Idle";
        [SerializeField] private string _walkStateName = "Mage_Walk";
        [SerializeField] private string _runStateName = "Mage_Run";
        [SerializeField] private float _moveThreshold = 0.1f;
        [SerializeField] private float _crossFadeDuration = 0.12f;

        private readonly HashSet<int> _parameterHashes = new();
        private int _currentStateHash;

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

            if (!_parameterHashes.Contains(SpeedHash))
            {
                CrossFadeMovementState();
            }
        }

        private void CrossFadeMovementState()
        {
            string stateName = GetMovementStateName();
            if (!TryGetStateHash(stateName, out int stateHash))
            {
                return;
            }

            if (_currentStateHash == stateHash)
            {
                return;
            }

            _currentStateHash = stateHash;
            _animator.CrossFadeInFixedTime(stateHash, _crossFadeDuration, BaseLayerIndex);
        }

        private string GetMovementStateName()
        {
            if (_movement.MoveAmount <= _moveThreshold)
            {
                return _idleStateName;
            }

            if (_movement.IsRunning && TryGetStateHash(_runStateName, out _))
            {
                return _runStateName;
            }

            return _walkStateName;
        }

        private bool TryGetStateHash(string stateName, out int stateHash)
        {
            stateHash = Animator.StringToHash($"{_baseLayerName}.{stateName}");
            return !string.IsNullOrWhiteSpace(stateName) && _animator.HasState(BaseLayerIndex, stateHash);
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
